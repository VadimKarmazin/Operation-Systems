using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OS_3
{
  class Producer
  {
    private ChannelWriter<int> Writer;
    public Producer(ChannelWriter<int> _writer, CancellationToken tok)
    {
      Writer = _writer;
      Task.WaitAll(Run(tok));
    }

    private async Task Run(CancellationToken tok)
    {
      var r = new Random();
      while (await Writer.WaitToWriteAsync())
      {
        if (tok.IsCancellationRequested)
        {
          Console.WriteLine("\tПроизводитель остановлен.");
          return;
        }
        if (Program.flag && Program.count <= 100)
        {
          var item = r.Next(1, 101);
          await Writer.WriteAsync(item);
          Program.count += 1;
          Console.WriteLine($"\tЗаписанные данные: {item}");
        }
      }
    }
  }

  class Consumer
  {
    private ChannelReader<int> Reader;

    public Consumer(ChannelReader<int> _reader, CancellationToken tok)
    {
      Reader = _reader;
      Task.WaitAll(Run(tok));
    }

    private async Task Run(CancellationToken tok)
    {
      while (await Reader.WaitToReadAsync())
      {
        if (Reader.Count != 0)
        {
          var item = await Reader.ReadAsync();
          Program.count -= 1;
          Console.WriteLine($"\tПолученные данные: {item}");
        }
        if (Reader.Count >= 100)
        {
          Program.flag = false;
        }
        else if (Reader.Count <= 80)
        {
          Program.flag = true;
        }
        if (tok.IsCancellationRequested)
        {
          if (Reader.Count == 0)
          {
            Console.WriteLine("\tПотребитель остановлен. ");
            return;
          }
        }
      }
    }
  }

  class Program
  {
    static void program()
    {
            Channel<int> channel = Channel.CreateBounded<int>(200);
            var cts = new CancellationTokenSource();
            Task[] streams = new Task[5];
            for (int i = 0; i < 5; i++)
            {
              if (i < 3)
              {
                streams[i] = Task.Run(() => { new Producer(channel.Writer, cts.Token); }, cts.Token);
              }
              else
              {
                streams[i] = Task.Run(() => { new Consumer(channel.Reader, cts.Token); }, cts.Token);
              }
            }
            new Thread(() =>
            {
              for (; ; )
              {
                if (Console.ReadKey(true).Key == ConsoleKey.Q)
                {
                  cts.Cancel();
                }
              }
            })
            { IsBackground = true }.Start();
            Task.WaitAll(streams); 
    }

    static void Main(string[] args)
    {
      program();
    }
}
