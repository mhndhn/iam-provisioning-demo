namespace Provisioning.Core;

public enum RequestStatus { Pending, Processing, Completed, Retry, Failed }

public class ProvisioningRequest
{
    public const int MaxAttempts = 3;
    public Guid Id { get; } = Guid.NewGuid();
    public string UserId { get; init; } = "";
    public string TargetSystem { get; init; } = "";   // e.g. "ActiveDirectory", "IBMi", "AWD"
    public RequestStatus Status { get; private set; } = RequestStatus.Pending;
    public int Attempts { get; private set; }

    public void StartProcessing()
    {
        if (Status is not (RequestStatus.Pending or RequestStatus.Retry))
            throw new InvalidOperationException($"Cannot start from {Status}");
        Status = RequestStatus.Processing;
        Attempts++;
    }

    public void Complete()
    {
        EnsureProcessing();
        Status = RequestStatus.Completed;
    }

    public void Fail()
    {
        EnsureProcessing();
        Status = Attempts >= MaxAttempts ? RequestStatus.Failed : RequestStatus.Retry;
    }

    private void EnsureProcessing()
    {
        if (Status != RequestStatus.Processing)
            throw new InvalidOperationException($"Request is {Status}, not Processing");
    }
}
