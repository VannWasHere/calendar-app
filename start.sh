#!/usr/bin/env sh
set -eu

dotnet restore
dotnet run --no-restore --launch-profile http -- --seed
dotnet run --no-restore --launch-profile http
