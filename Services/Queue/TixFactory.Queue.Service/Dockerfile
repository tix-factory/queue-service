FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

# Copies the contents of the directory the Dockerfile is in into the image.
COPY ./Application /Application

# Set working directory for the ENTRYPOINT
WORKDIR Application

# Run the application
ENTRYPOINT ["dotnet", "TixFactory.Queue.Service.dll"]
