#!/bin/bash

# Script to run the ShareMeal project smoothly

echo "Starting ShareMeal Platform..."

# Build and run the project
dotnet run --project "ShareMeal.Web.csproj" --launch-profile "http"
