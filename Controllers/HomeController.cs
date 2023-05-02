using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using MusicBase.Models;
using System.Diagnostics;

namespace MusicBase.Controllers
{
	public class HomeController : Controller
	{

		static bool isSet = false;
		ServiceBusClient client;
		ServiceBusSender sender;
		ServiceBusProcessor processor;
		static int messagesInQueue = 1000;
		static bool accessEnabled = false;
		private const string queueConnectionString = "Endpoint=sb://musicbasequeue.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=wy9ASUwncV0cExwUiIVIl/LZmHss9HYjEQ1qV7vf89c=";
		private const string toWebsiteQueue = "a";
		private const string toLoadBalancerQueue = "b";

		public HomeController() { }

		public IActionResult Index()
		{
			if (!accessEnabled)
				return RedirectToAction(nameof(Lobby));
			return View();
		}

		public async Task<IActionResult> Lobby()
		{
			if (accessEnabled)
				return RedirectToAction(nameof(Index));
			if (!isSet)
			{
				isSet = true;
				CreateBusClient();
				WaitToQueueEnd();
			}
			return View();
		}

		private async Task WaitToQueueEnd()
		{
			using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
			if (!messageBatch.TryAddMessage(new ServiceBusMessage("Enqueue")))
			{
				throw new Exception($"The message is too large to fit in the batch.");
			}
			Console.WriteLine("Enqueue");
			await sender.SendMessagesAsync(messageBatch);
			processor.StartProcessingAsync();
			while (!accessEnabled)
			{
				if (!messageBatch.TryAddMessage(new ServiceBusMessage("NumberInQueue")))
				{
					throw new Exception($"The message is too large to fit in the batch.");
				}
				await sender.SendMessagesAsync(messageBatch);
				Thread.Sleep(3000);
			}
			Console.WriteLine("NumberInQueue");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		void CreateBusClient()
		{
			ServiceBusClientOptions clientOptions = new()
			{
				TransportType = ServiceBusTransportType.AmqpWebSockets
			};

			client = new ServiceBusClient(queueConnectionString, clientOptions);
			sender = client.CreateSender(toLoadBalancerQueue);

			processor = client.CreateProcessor(toWebsiteQueue, new ServiceBusProcessorOptions() { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });
			processor.ProcessMessageAsync += MessageHandler;
			processor.ProcessErrorAsync += ErrorHandler;
		}

		async Task MessageHandler(ProcessMessageEventArgs args)
		{
			int a = int.Parse(args.Message.Body.ToString());
			messagesInQueue = a < messagesInQueue ? a : messagesInQueue;
			Console.WriteLine(messagesInQueue);
			if (messagesInQueue < 2)
			{
				accessEnabled = true;
				processor.StopProcessingAsync();
			}
		}

		Task ErrorHandler(ProcessErrorEventArgs args)
		{
			Console.WriteLine(args.Exception.ToString());
			return Task.CompletedTask;
		}

		[HttpPost]
		public int GetMessagesCount()
		{
			return messagesInQueue;
		}
	}
}
//185