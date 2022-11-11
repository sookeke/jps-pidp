#!/usr/bin/env bash
set -m
# wait for startup
sleep 30
./opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Pa55w0rd -i setup.sql
