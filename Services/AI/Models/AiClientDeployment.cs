namespace NoMoreLegacy.Services.AI;

public sealed class AiClientDeployment
{
    private readonly string _deploymentName;

    public string DeploymentName => _deploymentName;

    private AiClientDeployment(string deploymentName)
    {
        _deploymentName = deploymentName;
    }

    // The public, static, and readonly "enum" members
    public static readonly AiClientDeployment Gpt5Mini = new AiClientDeployment("gpt-5-mini");
    public static readonly AiClientDeployment Gpt35Turbo = new AiClientDeployment("gpt-35-turbo");
    public static readonly AiClientDeployment Gpt41Mini = new AiClientDeployment("gpt-4.1-mini");
}