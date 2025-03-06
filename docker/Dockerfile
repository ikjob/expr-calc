# special docker that helps to restore packages efficiently
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore_backend
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global --no-cache dotnet-subset
WORKDIR /restore
COPY ./src/backend /restore/backend
RUN dotnet subset restore backend/ExprCalc/ExprCalc.csproj \
  --root-directory /restore --output restore_subset/

# Build backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build_backend

COPY --from=restore_backend /restore/restore_subset/backend /src/backend
RUN dotnet restore -r linux-x64 /src/backend/ExprCalc/

COPY ./src/backend /src/backend
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /publish/app /src/backend/ExprCalc \
    && rm -f /publish/app/*.pdb \
    && rm -f /publish/app/appsettings.json \
    && rm -f /publish/app/appsettings.Development.json \
    && rm -f /publish/app/wwwroot/*


# Build frontend
FROM node:23-alpine AS build_frontend

COPY ./src/frontend/package.json ./src/frontend/yarn.lock /src/frontend/
WORKDIR /src/frontend/
RUN yarn install

COPY ./src/frontend .
RUN yarn build


# Final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim

COPY --from=build_backend /publish /home
WORKDIR /home/app/

COPY --from=build_frontend /src/frontend/dist /home/app/wwwroot/

COPY ./docker/appsettings.json /home/app/

EXPOSE 80
ENTRYPOINT ["/home/app/ExprCalc"]
CMD ["--config /home/app/appsettings.json"]