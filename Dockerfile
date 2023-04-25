# .NET SDK 官方镜像:
# https://mcr.microsoft.com/product/dotnet/sdk/about

# 使用微软官方的 .NET6 SDK 容器镜像来编译此工程
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
# 设置工作目录为容器内的 /src 路径
WORKDIR /src
# 先将工程设置文件 .csproj 复制过去
COPY *.csproj .
# 根据 .csproj 文件来拉取可能存在的 nuget package 依赖
RUN dotnet restore
# 将整个工程都复制过去 (除了那些被 .dockerignore 忽略的)
COPY . .
# 编译工程, 并将编译好的文件输出到 /publish 路径下
RUN dotnet publish -c Release -o /publish

# 使用微软官方的 .NET6 Runtime 容器镜像作为基础来制作用于发布的镜像
FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
# 如果运行时不需要 AST.NET Runtime, 则可以使用更小的镜像:
# FROM mcr.microsoft.com/dotnet/runtime:6.0 as runtime

# 将容器内的 /publish 路径设置为工作路径
WORKDIR /publish
# 将那些在上一个容器中被编译好的文件，复制到新的容器的工作路径中
COPY --from=builder /publish .
# 设置环境变量 MONGODB_URI 的默认值
ENV MONGODB_URI=mongodb://mongo:27017/
# 设置环境变量 DB_NAME 的默认值
ENV DB_NAME=web-app-demo
# 暴露 3000 端口
EXPOSE 3000
# 在工作目录中执行 dotnet WebAppDemo.dll 命令
CMD [ "dotnet", "WebAppDemo.dll" ]