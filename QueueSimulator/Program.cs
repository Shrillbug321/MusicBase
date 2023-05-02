using Azure.Messaging.ServiceBus;

ServiceBusClient client;
ServiceBusSender sender;
ServiceBusProcessor processor;
ServiceBusReceiver receiver;
bool enqueeded = false;
Random random = new();
const string toWebsiteQueue = "a";
const string toLoadBalancerQueue = "b";
const string queueConnectionString = "Endpoint=sb://musicbasequeue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wy9ASUwncV0cExwUiIVIl/LZmHss9HYjEQ1qV7vf89c=";
CreateBusClient();
ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
int messagesInQueue = 1000;
processor = client.CreateProcessor(toLoadBalancerQueue, new ServiceBusProcessorOptions(){ ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });
receiver = client.CreateReceiver("accessQueue",
new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });
processor.ProcessMessageAsync += MessageHandler;
processor.ProcessErrorAsync += ErrorHandler;
processor.StartProcessingAsync();
while (true) ;

void CreateBusClient()
{
	ServiceBusClientOptions clientOptions = new()
	{
		TransportType = ServiceBusTransportType.AmqpWebSockets
	};

	client = new ServiceBusClient(queueConnectionString, clientOptions);
	sender = client.CreateSender(toWebsiteQueue);
}

async Task MessageHandler(ProcessMessageEventArgs args)
{
	switch (args.Message.Body.ToString())
	{
		case "Enqueue":
			if (!enqueeded)
			{
				messagesInQueue = random.Next(25, 50);
				Console.WriteLine("Enqueue");
				enqueeded = true;
			}
			break;
		case "NumberInQueue":
			break;
		default:
			return;
	}
	messagesInQueue -= random.Next(1, 3);
	if (!messageBatch.TryAddMessage(new ServiceBusMessage(messagesInQueue.ToString())))
	{
		throw new Exception($"The message is too large to fit in the batch.");
	}
	await sender.SendMessagesAsync(messageBatch);
	Console.WriteLine(messagesInQueue);
	if (messagesInQueue <= 0)
		await processor.StopProcessingAsync();
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
	Console.WriteLine(args.Exception.ToString());
	return Task.CompletedTask;
}
//106