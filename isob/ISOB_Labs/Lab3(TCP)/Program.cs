using Lab3;


CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
CancellationToken token = cancelTokenSource.Token;

Client client = new Client();
NetworkNode server = new NetworkNode();
CyberIntruder hacker = new CyberIntruder();

try
{
    Task.WaitAll([server.Listen(token),
        hacker.ConnectToServer(NetworkNode.ServerIP, token),
        client.ConnectToServer(NetworkNode.ServerIP, cancelTokenSource)]);

    Thread.Sleep(500);
}
catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
{ }