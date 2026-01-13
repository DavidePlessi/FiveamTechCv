
cd ../src/fiveamtechcv-web
call yarn install
call yarn run build

cd ../../

rmdir .\publish  /s /q

dotnet publish ./src/FiveamTechCv.Server/FiveamTechCv.Server.csproj -o ./publish -f net8.0 -c Release -p:WarningLevel=0 -r linux-x64 --no-self-contained

robocopy ./src/fiveamtechcv-web/dist/ ./publish/wwwroot/ /MIR

cd ./Docker
docker compose down
docker compose build

docker compose push