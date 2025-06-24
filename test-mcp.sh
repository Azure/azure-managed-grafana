#!/bin/bash
# Test script for Blue Bridge MCP integration
# This script helps verify the MCP configuration

echo "=== Blue Bridge MCP Test Script ==="
echo

# Check if mcp.json exists
if [ -f "mcp.json" ]; then
    echo "✓ mcp.json configuration file found"
else
    echo "✗ mcp.json configuration file not found"
    exit 1
fi

# Validate JSON structure
if python3 -m json.tool mcp.json > /dev/null 2>&1; then
    echo "✓ mcp.json has valid JSON structure"
else
    echo "✗ mcp.json has invalid JSON structure"
    exit 1
fi

# Check if binary exists
if [ -f "bluebridge-cli-linux-x64" ]; then
    echo "✓ Blue Bridge CLI binary found"
else
    echo "✗ Blue Bridge CLI binary not found"
    echo "  Run: wget -O bluebridge-cli-linux-x64 https://github.com/Azure/blue-bridge/releases/download/v0.0.1/bluebridge-cli-linux-x64"
    exit 1
fi

# Check if binary is executable
if [ -x "bluebridge-cli-linux-x64" ]; then
    echo "✓ Blue Bridge CLI binary is executable"
else
    echo "! Blue Bridge CLI binary is not executable, making it executable..."
    chmod +x bluebridge-cli-linux-x64
fi

echo
echo "=== MCP Configuration ==="
cat mcp.json
echo

echo "=== Test Azure Resource Graph Query ==="
echo "To test the MCP function, use the Copilot coding agent with:"
echo "blue_bridge_query_azure_resource_graph"
echo 
echo "Sample query:"
echo "ResourceContainers"
echo "| where type =~ 'microsoft.resources/subscriptions'"
echo "| project subscriptionId, subscriptionName = name, tags"
echo

echo "Note: The binary may require Azure authentication context to function properly."
echo "Ensure you are logged in to Azure CLI: az login"