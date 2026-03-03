using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Logging;
using TechRadar.Core.Domain;
using TechRadar.Core.Interfaces;

namespace TechRadar.Core.Services;

/// <summary>
/// Classifies discovered technology signals using the Anthropic Claude API.
/// Used when ANTHROPIC_API_KEY is present in configuration.
/// </summary>
public class AnthropicLlmClassifier(string apiKey, string model) : ILlmClassifier
{
    private readonly AnthropicClient _client = new(apiKey);

    private const string SystemPrompt = """
        You are an IoT and connectivity technology analyst for a Technology Radar.
        The radar has four quadrants:
        - ConnectivityProtocols: wireless/wired protocols (MQTT, Zigbee, Matter, Thread, LoRaWAN, NB-IoT, CoAP, Z-Wave, Bluetooth Mesh)
        - EdgePlatforms: edge compute and cloud IoT platforms (AWS IoT, Azure IoT Hub, EdgeX, Eclipse Ditto)
        - ToolsAndFrameworks: development tools, SDKs, RTOSes (Zephyr, Arduino, ESP-IDF, OpenThread, FreeRTOS)
        - StandardsAndTechniques: specifications and patterns (OPC-UA, FIWARE, DTDL, NGSI-LD, Modbus)

        The radar has four rings:
        - Adopt: proven, recommended for production use
        - Trial: worth pursuing in a low-risk project
        - Assess: worth exploring to understand potential
        - Hold: proceed with caution; not recommended for new projects

        Given a technology name, description, and source URLs, respond with ONLY valid JSON:
        {"quadrant":"ConnectivityProtocols","ring":"Adopt","evidenceSummary":"...","confidenceScore":0.85}

        Quadrant must be one of: ConnectivityProtocols, EdgePlatforms, ToolsAndFrameworks, StandardsAndTechniques
        Ring must be one of: Adopt, Trial, Assess, Hold
        evidenceSummary: 2-3 sentence synthesis explaining the recommendation
        confidenceScore: 0.0-1.0 float

        If the technology is clearly not IoT/connectivity related, respond with:
        {"quadrant":null,"ring":null,"evidenceSummary":null,"confidenceScore":0.0}
        """;

    public async Task<ClassificationResult> ClassifyAsync(
        string name, string rawDescription, IEnumerable<string> sourceUrls,
        CancellationToken ct = default)
    {
        var urls = string.Join(", ", sourceUrls.Take(3));
        var userContent = $"Technology: {name}\nDescription: {rawDescription}\nSources: {urls}";

        try
        {
            var response = await _client.Messages.GetClaudeMessageAsync(
                new MessageParameters
                {
                    Model = model,
                    MaxTokens = 512,
                    System = [new SystemMessage(SystemPrompt)],
                    Messages = [new Message { Role = RoleType.User, Content = [new TextContent { Text = userContent }] }]
                }, ct);

            var json = response.Content.OfType<TextContent>().FirstOrDefault()?.Text ?? string.Empty;
            var parsed = JsonSerializer.Deserialize<LlmResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed == null || parsed.Quadrant == null || parsed.Ring == null)
                return new ClassificationResult(null, null, null, 0f, false);

            if (!Enum.TryParse<Quadrant>(parsed.Quadrant, out var quadrant))
                return new ClassificationResult(null, null, null, 0f, false);

            if (!Enum.TryParse<Ring>(parsed.Ring, out var ring))
                return new ClassificationResult(null, null, null, 0f, false);

            return new ClassificationResult(quadrant, ring, parsed.EvidenceSummary, parsed.ConfidenceScore, true);
        }
        catch
        {
            return new ClassificationResult(null, null, null, null, false);
        }
    }

    private record LlmResponse(
        string? Quadrant, string? Ring, string? EvidenceSummary, float? ConfidenceScore);
}
