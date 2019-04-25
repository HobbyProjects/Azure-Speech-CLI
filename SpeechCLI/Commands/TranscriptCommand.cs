﻿using CRIS;
using CRIS.Models;
using SpeechCLI.Attributes;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SpeechCLI.Utils;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;

namespace SpeechCLI.Commands
{
    [Command(Description = "Batch transcription.")]
    [Subcommand("create", typeof(Create))]
    [Subcommand("list", typeof(List))]
    [Subcommand("show", typeof(Show))]
    //[Subcommand("status", typeof(Status))]
    [Subcommand("delete", typeof(Delete))]
    [Subcommand("download", typeof(Download))]
    [Subcommand("single", typeof(Single))]
    class TranscriptCommand : SpeechCommandBase
    {
        public TranscriptCommand(ISpeechServicesAPIv20 speechApi, IConsole console, Config config) : base(speechApi, console, config) { }

        [Command(Description = "Start new batch transcription.")]
        class Create : ParamActionCommandBase
        {
            [Option(Description = "(Required) Name of this transcription.")]
            [Required]
            string Name { get; set; }

            [Option(Description = "Description of this transcription.")]
            string Description { get; set; }

            [Option(Description = "Language. Default: en-us")]
            string Locale { get; set; }

            [Option(ValueName = "URL", Description = "URL pointing to audio file.")]
            string Recording { get; set; }

            [Option(ValueName = "GUID", Description = "ID of the acoustic model to use. Run 'model list' to get your models. Default: baseline.")]
            [Guid]
            string Model { get; set; }

            [Option(ShortName = "lng", LongName = "language", ValueName = "GUID", Description = "ID of language model to use. Run 'model list' to get your models. Default: baseline.")]
            string LanguageModel { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Will stop and wait for transcription to be ready.")]
            bool Wait { get; set; }

            [Option(Description = "Custom properties of this transcription. Format: '--properties prop1=val1;prop2=val2;prop3=val3'")]
            string Properties { get; set; }

            int OnExecute()
            {
                var definition = new TranscriptionDefinition(Recording, Locale ?? "en-us", Name, properties: SplitProperties(Properties));
                definition.ModelsProperty = new List<ModelIdentity>();

                if (!string.IsNullOrEmpty(Model))
                    definition.ModelsProperty.Add(new ModelIdentity(Guid.Parse(Model)));

                if (!string.IsNullOrEmpty(LanguageModel))
                    definition.ModelsProperty.Add(new ModelIdentity(Guid.Parse(LanguageModel)));

                _console.WriteLine("Creating transcript...");
                var res = CreateAndWait(
                    () => _speechApi.CreateTranscription(definition), 
                    Wait, 
                    _speechApi.GetTranscription);

                return res;
            }
        }

        [Command(Description = "Get a list of batch transcriptions.")]
        class List
        {
            int OnExecute()
            {
                _console.WriteLine("Getting transcriptions...");

                var res = CallApi<List<Transcription>>(_speechApi.GetTranscriptions);
                if (res == null)
                    return -1;

                if (res.Count == 0)
                {
                    _console.WriteLine("No transcriptions found.");
                }
                else
                {
                    foreach (var t in res)
                    {
                        _console.WriteLine($"{t.Id,30} {t.Name} {t.Status}");
                    }
                }

                return 0;
            }
        }

        [Command(Description = "Show specific batch transcription.")]
        class Show : ParamActionCommandBase
        {
            [Argument(0, Name = "GUID", Description = "ID of transcription. Use 'batch list' to get your transcriptions.")]
            [Guid]
            [Required]
            string Id { get; set; }

            int OnExecute()
            {
                _console.WriteLine("Getting transcription...");

                var res = CallApi<Transcription>(() => _speechApi.GetTranscription(Guid.Parse(Id)));
                if (res == null)
                    return -1;

                _console.WriteLine(SafeJsonConvert.SerializeObject(res, new Newtonsoft.Json.JsonSerializerSettings() { Formatting = Newtonsoft.Json.Formatting.Indented }));

                return 0;
            }
        }

        [Command(Description = "Delete specific batch transcription.")]
        class Delete : ParamActionCommandBase
        {
            [Argument(0, Name = "GUID", Description = "ID of the transcription to delete.")]
            [Required]
            [Guid]
            string Id { get; set; }

            int OnExecute()
            {
                _console.WriteLine("Deleting transcription...");
                var res = CallApi(() => _speechApi.DeleteTranscription(Guid.Parse(Id)));
                _console.WriteLine("Done.");

                return res;
            }
        }

        [Command(Description = "Download results of finished transcription.")]
        class Download : ParamActionCommandBase
        {
            [Option(ValueName = "GUID", Description = "ID of the transcription to download.")]
            [Required]
            [Guid]
            string Id { get; set; }

            [Option(Description = "Output directory where transcriptions will be downloaded.")]
            [Required]
            [LegalFilePath]
            string OutDir { get; set; }

            [Option(ValueName = "JSON|VTT", Description = "(Optional) Output format. Currently supported are: VTT and JSON. Default = JSON")]
            string Format { get; set; }

            async Task<int> OnExecute()
            {
                // get transcript
                _console.WriteLine("Getting transcription...");
                var transcription = CallApi<Transcription>(() => _speechApi.GetTranscription(Guid.Parse(Id)));

                // read URLs (channels)
                foreach (var url in transcription.ResultsUrls)
                {
                    (string text, string extension) output;

                    using (var hc = new HttpClient())
                    {
                        // download URLs
                        var res = await hc.GetAsync(url.Value);
                        if (res.IsSuccessStatusCode)
                        {
                            var rawContent = await res.Content.ReadAsStringAsync();
                            var convertedContent = SafeJsonConvert.DeserializeObject<TranscriptionResult>(rawContent);
                            switch(Format?.ToLower())
                            {
                                case "vtt":
                                    output = new VttTranscriptParser().Parse(convertedContent);
                                    break;
                                case "json":
                                default:
                                    output = new JsonTranscriptParser().Parse(convertedContent);
                                    break;
                            }

                            // save to output
                            var outputFileName = Path.Join(OutDir, convertedContent.AudioFileResults[0].AudioFileName + output.extension);
                            File.WriteAllText(outputFileName, output.text);
                            _console.WriteLine($"File {outputFileName} written.");
                        }
                        else
                        {
                            return -1;
                        }
                    }
                }

                return 0;
            }

        }

        [Command(Description = "Run transcription on single, short file.")]
        class Single : ParamActionCommandBase
        {
            [Option(ShortName = "i", LongName = "input", Description = "WAV file for transcription.")]
            [Required]
            [FileExists]
            string InputFile { get; set; }

            [Option(Description = "Speech endpoint which should perform the transcription. If not specified, baseline will be used.")]
            string Endpoint { get; set; }

            [Option(ValueName = "Simple|Detailed", Description = "... Default: Simple")]
            OutputFormat OutputFormat { get; set; } = OutputFormat.Simple;

            async Task<int> OnExecute()
            {
                var speechConfig = SpeechConfig.FromSubscription(_config.SpeechKey, _config.SpeechRegion);
                speechConfig.OutputFormat = OutputFormat;
                if (Endpoint != null)
                {
                    speechConfig.EndpointId = Endpoint;
                }

                var audioConfig = AudioConfig.FromWavFileInput(InputFile);

                using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
                {
                    var result = await recognizer.RecognizeOnceAsync();

                    if (OutputFormat == OutputFormat.Simple)
                    {
                        switch (result.Reason) {
                            case ResultReason.RecognizedSpeech:
                                Console.WriteLine(result.Text);
                                break;
                            case ResultReason.NoMatch:
                                Console.WriteLine($"Speech could not be recognized. (NoMatch)");
                                break;
                            case ResultReason.Canceled:
                                var cancellation = CancellationDetails.FromResult(result);
                                Console.WriteLine($"Recognition was canceled. (CANCELED)");

                                if (cancellation.Reason == CancellationReason.Error)
                                {
                                    Console.WriteLine($"CANCELED: ErrorCode: {cancellation.ErrorCode}");
                                    Console.WriteLine($"CANCELED: ErrorDetails: {cancellation.ErrorDetails}");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine(SafeJsonConvert.SerializeObject(result));
                    }
                }

                return 0;
            }

        }
    }
}
