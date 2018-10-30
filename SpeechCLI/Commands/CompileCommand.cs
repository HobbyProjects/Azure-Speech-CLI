﻿using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace CustomSpeechCLI.Commands
{
    [Command(Description = "Helper to compile WAV samples and transcripts.")]
    class CompileCommand
    {
        [Option(LongName = "audio", ShortName = "a", ValueName = "FOLDER", Description = "Path to folder where all WAV files are.")]
        [Required]
        [DirectoryExists]
        string AudioPath { get; set; }

        [Option(LongName = "transcript", ShortName = "t", ValueName = "FILE", Description = "Path to the TXT file with transcripts. One line per WAV file.")]
        [Required]
        [FileExists]
        string TranscriptPath { get; set; }

        [Option(LongName = "output", ShortName = "o", ValueName = "FOLDER", Description = "Target folder. Will be created if non-existent. Default: audio folder")]
        string OutputPath { get; set; }

        [Option(LongName = "test-percentage", ShortName = "tp", ValueName = "PERCENTAGE", Description = "What portion (in %) of source data will be split as test dataset. Default: 10")]
        int? TestPercentage { get; set; }

        int OnExecute()
        {
            Console.WriteLine("Compiling files...");

            var files = Directory.GetFiles(AudioPath);
            var lines = File.ReadAllLines(TranscriptPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray(); // not interested in empty lines

            //TODO: kontrola BOM, případně přidat BOM
            //TODO: vynechat řádky, které nemají WAV

            var numberOfTests = (int)Math.Round((double)(files.Length * (TestPercentage ?? 10) / 100));
            Console.WriteLine($"Found {files.Length} files and {lines.Length} text lines. {numberOfTests} will be used as test dataset.");

            var outputFolder = OutputPath ?? Path.Join(AudioPath, "Output");
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            //TODO: refaktorovat
            try
            {
                var trainFiles = files.Take(files.Length - numberOfTests);
                var trainLines = lines.Take(lines.Length - numberOfTests);
                var trainFolder = Path.Join(outputFolder, "Train");

                Console.WriteLine("Copying and ZIPing training files.");
                CreateFolderAndCopyFiles(trainFolder, trainFiles);
                ZipFile.CreateFromDirectory(trainFolder, Path.Join(outputFolder, "Train.zip"), CompressionLevel.Fastest, false);
                File.WriteAllLines(Path.Join(outputFolder, "train.txt"), trainLines);

                var testFiles = files.Reverse().Take(numberOfTests);
                var testLines = lines.Reverse().Take(numberOfTests);
                var testFolder = Path.Join(outputFolder, "Test");

                Console.WriteLine("Copying and ZIPing testing files.");
                CreateFolderAndCopyFiles(testFolder, testFiles);
                ZipFile.CreateFromDirectory(testFolder, Path.Join(outputFolder, "Test.zip"), CompressionLevel.Fastest, false);
                File.WriteAllLines(Path.Join(outputFolder, "test.txt"), testLines);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Done.");

            return 1;
        }

        void CreateFolderAndCopyFiles(string folder, IEnumerable<string> files)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (var f in files)
            {
                File.Copy(f, Path.Combine(folder, Path.GetFileName(f)), true);
            }
        }
    }
}
