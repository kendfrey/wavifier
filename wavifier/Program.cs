using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wavifier
{
	class Program
	{
		static void Main(string[] args)
		{
			string rateArg = args.Where((s, i) => i > 0 && (args[i - 1] == "-r" || args[i - 1] == "--rate")).FirstOrDefault() ?? "44100";
			int rate;
			if (!int.TryParse(rateArg, out rate) || rate <= 0)
			{
				Console.Error.WriteLine("Unsupported sample rate " + rateArg);
				return;
			}
			string sizeArg = args.Where((s, i) => i > 0 && (args[i - 1] == "-s" || args[i - 1] == "--size")).FirstOrDefault() ?? "8";
			int size;
			if (!int.TryParse(sizeArg, out size) || size % 8 != 0 || size <= 0)
			{
				Console.Error.WriteLine("Unsupported sample size " + sizeArg);
				return;
			}
			int sizeBytes = size / 8;
			string channelsArg = args.Where((s, i) => i > 0 && (args[i - 1] == "-c" || args[i - 1] == "--channels")).FirstOrDefault() ?? "1";
			short channels;
			if (!short.TryParse(channelsArg, out channels) || channels <= 0)
			{
				Console.Error.WriteLine("Unsupported channel count " + channelsArg);
				return;
			}
			List<byte> data = new List<byte>(1024);
			using (Stream input = Console.OpenStandardInput())
			{
				int b = input.ReadByte();
				while (b >= 0)
				{
					data.Add((byte)b);
					b = input.ReadByte();
				}
			}
			using (Stream output = Console.OpenStandardOutput())
			{
				StreamWriter textOutput = new StreamWriter(output, Encoding.ASCII, 4, true);
				BinaryWriter binaryOutput = new BinaryWriter(output, Encoding.ASCII, true);
				textOutput.AutoFlush = true;

				textOutput.Write("RIFF"); // RIFF header chunk
				binaryOutput.Write(36 + data.Count); // chunk size
				textOutput.Write("WAVE"); // file format

				textOutput.Write("fmt "); // fmt chunk
				binaryOutput.Write(16); // chunk size
				binaryOutput.Write((short)1); // PCM format
				binaryOutput.Write(channels); // channel count
				binaryOutput.Write(rate); // sample rate
				binaryOutput.Write(rate * channels * sizeBytes); // byte rate
				binaryOutput.Write((short)(channels * sizeBytes)); // bytes per sample (all channels)
				binaryOutput.Write((short)size); // bits per sample (per channel)

				textOutput.Write("data"); // data chunk
				binaryOutput.Write(data.Count); // chunk size
				binaryOutput.Write(data.ToArray()); // sound data
			}
		}
	}
}
