# SimpleMCP

Skeleton of an MCP (Model Context Protocol) server built with .NET, intended as a starting point for building custom tools that Claude Desktop can call locally.

## Publishing as a self-contained executable

Run the following command from the repo root to produce a single binary with no external dependencies.

**Windows:**
```bash
dotnet publish SimpleMCP.Server/SimpleMCP.Server.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o C:\LocalMCPServer
```

The output folder (`C:\LocalMCPServer`) will contain `SimpleMCP.Server.exe`.

**macOS (Intel):**
```bash
dotnet publish SimpleMCP.Server/SimpleMCP.Server.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o ~/LocalMCPServer
```

**macOS (Apple Silicon):**
```bash
dotnet publish SimpleMCP.Server/SimpleMCP.Server.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ~/LocalMCPServer
```

The output folder (`~/LocalMCPServer`) will contain `SimpleMCP.Server`.

## Configuring Claude Desktop

1. Open **Claude Desktop**.
2. Go to **Settings → Developer**.
3. Click **Edit Config** to open `claude_desktop_config.json`.
4. Add your server under `mcpServers`:

**Windows:**
```json
{
  "mcpServers": {
    "localMCP": {
      "command": "C:\\LocalMCPServer\\SimpleMCP.Server.exe"
    }
  }
}
```

**macOS:**
```json
{
  "mcpServers": {
    "localMCP": {
      "command": "/Users/your-username/LocalMCPServer/SimpleMCP.Server"
    }
  }
}
```

5. Save the file and restart Claude Desktop. The server will appear under the MCP tools icon in the chat input bar.
