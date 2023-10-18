FROM mcr.microsoft.com/dotnet/aspnet:7.0
ENV SERVER_CONTENT_ROOT /app/public

COPY dist/ /app

WORKDIR /app
CMD /app/Server