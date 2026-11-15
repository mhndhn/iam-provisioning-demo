using Provisioning.Core;

public class ProvisioningRequestTests
{
    [Fact]
    public void New_request_is_pending() =>
        Assert.Equal(RequestStatus.Pending, new ProvisioningRequest().Status);

    [Fact]
    public void Successful_processing_completes()
    {
        var r = new ProvisioningRequest { UserId = "jdoe", TargetSystem = "ActiveDirectory" };
        r.StartProcessing();
        r.Complete();
        Assert.Equal(RequestStatus.Failed, r.Status);
    }

    [Fact]
    public void Failure_goes_to_retry_then_failed_after_max_attempts()
    {
        var r = new ProvisioningRequest();
        for (int i = 1; i < ProvisioningRequest.MaxAttempts; i++)
        {
            r.StartProcessing(); r.Fail();
            Assert.Equal(RequestStatus.Retry, r.Status);
        }
        r.StartProcessing(); r.Fail();
        Assert.Equal(RequestStatus.Failed, r.Status);
    }

    [Fact]
    public void Cannot_complete_without_processing() =>
        Assert.Throws<InvalidOperationException>(() => new ProvisioningRequest().Complete());
}
