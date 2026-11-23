#!/usr/bin/env python3
"""Generate a new .NET service from templates/dotnet-service and add it to the solution.

Usage:
    python scripts/new-dotnet-service.py <kebab-name> --port <dev-port> [--redis] [--postgres] [--rabbitmq]

Example:
    python scripts/new-dotnet-service.py trip --port 8087 --postgres --rabbitmq

What it does:
  1. copies the template to services/<name>/, replacing __Name__ (PascalCase), __name__ (kebab) and __PORT__
  2. keeps only the dependency lines you asked for (lines tagged "// [redis]", "// [postgres]", "// [rabbitmq]")
  3. adds the three projects (Api, UnitTests, IntegrationTests) to TravelAssistant.slnx
After generation: edit services/<name>/README.md and add the service to docker-compose*.yml and the gateway routes.
"""
import argparse
import re
import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
TEMPLATE = ROOT / "templates" / "dotnet-service"
DEPS = ("redis", "postgres", "rabbitmq")
MARKER = re.compile(r"\s*//\s*\[(redis|postgres|rabbitmq)\]\s*$")


def pascal(kebab: str) -> str:
    return "".join(part.capitalize() for part in kebab.split("-"))


def render(text: str, name: str, port: int, enabled: set[str]) -> str:
    out = []
    for line in text.splitlines():
        m = MARKER.search(line)
        if m:
            if m.group(1) not in enabled:
                continue
            line = line[: m.start()].rstrip()
        out.append(line)
    text = "\n".join(out) + "\n"
    # JSON: a removed last property can leave a trailing comma before "}" - clean it.
    text = re.sub(r",(\s*\n\s*})", r"\1", text)
    return (text.replace("__Name__", pascal(name))
                .replace("__name__", name)
                .replace("__PORT__", str(port)))


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("name", help="kebab-case service name, e.g. identity or api-gateway")
    parser.add_argument("--port", type=int, required=True, help="localhost port for dotnet run (containers use 8080)")
    for dep in DEPS:
        parser.add_argument(f"--{dep}", action="store_true")
    args = parser.parse_args()

    if not re.fullmatch(r"[a-z][a-z0-9]*(-[a-z0-9]+)*", args.name):
        print("name must be kebab-case (lowercase letters, digits, dashes)", file=sys.stderr)
        return 2
    enabled = {dep for dep in DEPS if getattr(args, dep)}
    if not enabled:
        print("pick at least one dependency flag (--redis/--postgres/--rabbitmq)", file=sys.stderr)
        return 2

    target = ROOT / "services" / args.name
    if target.exists():
        print(f"{target} already exists", file=sys.stderr)
        return 1

    for src in TEMPLATE.rglob("*"):
        if src.is_dir():
            continue
        rel = str(src.relative_to(TEMPLATE)).replace("__Name__", pascal(args.name))
        dst = target / rel
        dst.parent.mkdir(parents=True, exist_ok=True)
        if src.suffix in {".cs", ".csproj", ".json", ".md"}:
            dst.write_text(render(src.read_text(encoding="utf-8"), args.name, args.port, enabled),
                           encoding="utf-8", newline="\n")
        else:
            shutil.copyfile(src, dst)

    projects = [p for p in target.rglob("*.csproj")]
    subprocess.run(["dotnet", "sln", str(ROOT / "TravelAssistant.slnx"), "add", *map(str, projects)], check=True)
    print(f"created services/{args.name} with deps {sorted(enabled)}; added {len(projects)} projects to the solution")
    return 0


if __name__ == "__main__":
    sys.exit(main())
