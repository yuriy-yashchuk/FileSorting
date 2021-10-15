﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSorting
{
    class Program
    {
        const string _folderPath = @"c:\Temp";
        const string _inputFileName = "input.txt";
        const string _outputFileName = "output.txt";
        const int _chunkSizeLimit = 1000;//1000000000; //to keep chuck size around 1Gb

        static async Task Main(string[] args)
        {
            var inputFilePath = $"{_folderPath}\\{_inputFileName}";
            var outputFilePath = $"{_folderPath}\\{_outputFileName}";

            Generate(inputFilePath);

            var chunkFiles = await SplitFileAsync(inputFilePath);

            MergeFilesAsync(chunkFiles, outputFilePath);

            chunkFiles.ForEach(p => File.Delete(p));
        }

        private static void MergeFilesAsync(List<string> filePaths, string resultFilePath)
        {
            //initialize a reader for each file
            var readers = new List<StreamReader>();

            foreach(var filepath in filePaths)
            {
                readers.Add(new StreamReader(filepath));
            }

            var writer = new StreamWriter(resultFilePath);

            //initialize a structure with a reader and a newly read line from that reader
            var all = readers.Select((reader, index) => (reader, reader.ReadLine())).ToList();

            while (all.Count > 0)
            {
                //sort by string line
                all.Sort(CustomComparison);

                var el = all.First();

                //save the minimal element to the result file
                writer.WriteLine(el.Item2);
                
                //read a new line from a reader or remove it if there are no more lines
                if (el.reader.EndOfStream)
                {
                    all.Remove(el);
                }
                else
                {
                    all[0] = (el.reader, el.reader.ReadLine());
                }
            }

            readers.ForEach(r => r.Close());
            writer.Close();
        }

        private static async Task<List<string>> SplitFileAsync(string filePath)
        {
            var resultFilePaths = new List<string>();
            var reader = new StreamReader(filePath);

            int chunkIndex = 0;

            while (!reader.EndOfStream)
            {
                var chunkList = ReadChunk(reader, _chunkSizeLimit);

                chunkList.Sort(CustomComparison);

                var resFilePath = $"{_folderPath}\\chunk{chunkIndex}.txt";
                await File.WriteAllLinesAsync(resFilePath, chunkList);

                chunkIndex++;
                resultFilePaths.Add(resFilePath);
            }

            reader.Close();

            return resultFilePaths;
        }

        /// <summary>
        /// Reads from a StreamReader a chunk of data of specific size and returns as list of lines
        /// </summary>
        /// <param name="reader">StreamReader to read from</param>
        /// <param name="chunkSizeLimit">Approximate chunk size in bytes</param>
        /// <returns>List of lines read</returns>
        private static List<string> ReadChunk(StreamReader reader, int chunkSizeLimit)
        {
            var chunkList = new List<string>();
            int chunkSize = 0;

            while (!reader.EndOfStream && chunkSize < chunkSizeLimit)
            {
                var line = reader.ReadLine();
                chunkSize += line.Length + 2;//add 2 for \r\n
                chunkList.Add(line);
            }

            return chunkList;
        }

        /// <summary>
        /// Compares two string of format "XXX.AAA" first by "AAA" part, then by XXX
        /// </summary>
        private static int CustomComparison(string a, string b)
        {
            (var aN, var aS) = ParseNumberAndString(a);
            (var bN, var bS) = ParseNumberAndString(b);

            var res = string.Compare(aS, bS);

            if (res != 0)
            {
                return res;
            }
            else
            {
                if (aN == bN) return 0;
                if (aN > bN) return 1;
                return -1;
            }
        }

        /// <summary>
        /// Compares two structures by their second(string) part
        /// </summary>
        private static int CustomComparison<T>((T, string) a, (T, string) b)
        {
            return CustomComparison(a.Item2, b.Item2);
        }

        /// <summary>
        /// Parses string of format "XXX.AAA" into number XXX and string "AAA"
        /// </summary>
        private static (int, string) ParseNumberAndString(string s)
        {
            var dotIndex = s.IndexOf('.');
            var numberString = s.Substring(0, dotIndex);
            if (!int.TryParse(numberString, out var number))
            {
                Console.WriteLine($"Error with {s}");
            }

            return (number, s[(dotIndex + 1)..]);
        }

        private static void Generate(string filePath)
        {
            var writer = new StreamWriter(filePath);
            Random rnd = new Random();

            for (int i = 0; i < 1000; i++)//100000000 for 1 Gb
            {
                var n = rnd.Next(0, 1000);
                var str = $"{(char)rnd.Next(97, 122)}{(char)rnd.Next(97, 122)}{(char)rnd.Next(97, 122)}";
                writer.WriteLine($"{n}. {str}");
            }

            writer.Close();
        }
    }
}