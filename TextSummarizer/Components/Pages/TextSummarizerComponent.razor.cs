using Azure;
using System;
using Azure.AI.TextAnalytics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace TextSummarizer.Components.Pages
{
    public partial class TextSummarizerComponent : ComponentBase
    {
        private TextAnalyticsClient _client;
        private string _textAreaContent = @"The extractive summarization feature uses natural language processing techniques to locate key sentences in an unstructured text document. 
                These sentences collectively convey the main idea of the document. This feature is provided as an API for developers. 
                They can use it to build intelligent solutions based on the relevant information extracted to support various use cases. 
                Extractive summarization supports several languages. It is based on pretrained multilingual transformer models, part of our quest for holistic representations. 
                It draws its strength from transfer learning across monolingual and harness the shared nature of languages to produce models of improved quality and efficiency.";
        private string _summarizeText = string.Empty;

        public TextSummarizerComponent()
        {
            // This example requires environment variables named "LANGUAGE_KEY" and "LANGUAGE_ENDPOINT"
            string languageKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY");
            string languageEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT");

            AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
            Uri endpoint = new Uri(languageEndpoint);
            _client = new TextAnalyticsClient(endpoint, credentials);
        }

        protected override async Task OnInitializedAsync()
        {

        }

        public async Task SummarizeContentAsync()
        {
            Console.WriteLine(_textAreaContent);
            await TextSummarizationExampleAsync(_client);
        }


        //private async Task AfterUpload()
        //{
        //    var client = new TextAnalyticsClient(endpoint, credentials);
        //    await TextSummarizationExampleAsync(client);
        //}

        // Example method for summarizing text
        private async Task TextSummarizationExampleAsync(TextAnalyticsClient client)
        {
            //string document1 = @"The extractive summarization feature uses natural language processing techniques to locate key sentences in an unstructured text document. 
            //    These sentences collectively convey the main idea of the document. This feature is provided as an API for developers. 
            //    They can use it to build intelligent solutions based on the relevant information extracted to support various use cases. 
            //    Extractive summarization supports several languages. It is based on pretrained multilingual transformer models, part of our quest for holistic representations. 
            //    It draws its strength from transfer learning across monolingual and harness the shared nature of languages to produce models of improved quality and efficiency.";

            string document = _textAreaContent;

            // Prepare analyze operation input. You can add multiple documents to this list and perform the same
            // operation to all of them.
            var batchInput = new List<string>
            {
                document
            }; 

            TextAnalyticsActions actions = new TextAnalyticsActions()
            {
                ExtractiveSummarizeActions = new List<ExtractiveSummarizeAction>() { new ExtractiveSummarizeAction() }
            };

            // Start analysis process.
            AnalyzeActionsOperation operation = await client.StartAnalyzeActionsAsync(batchInput, actions);
            await operation.WaitForCompletionAsync();
            // View operation status.
            Console.WriteLine($"AnalyzeActions operation has completed");
            Console.WriteLine();

            Console.WriteLine($"Created On   : {operation.CreatedOn}");
            Console.WriteLine($"Expires On   : {operation.ExpiresOn}");
            Console.WriteLine($"Id           : {operation.Id}");
            Console.WriteLine($"Status       : {operation.Status}");

            Console.WriteLine();
            // View operation results.
            StringBuilder stringBuilder = new StringBuilder();
            await foreach (AnalyzeActionsResult documentsInPage in operation.Value)
            {
                IReadOnlyCollection<ExtractiveSummarizeActionResult> summaryResults = documentsInPage.ExtractiveSummarizeResults;

                foreach (ExtractiveSummarizeActionResult summaryActionResults in summaryResults)
                {
                    if (summaryActionResults.HasError)
                    {
                        Console.WriteLine($"  Error!");
                        Console.WriteLine($"  Action error code: {summaryActionResults.Error.ErrorCode}.");
                        Console.WriteLine($"  Message: {summaryActionResults.Error.Message}");
                        stringBuilder.Append($"Error! Action error code: {summaryActionResults.Error.ErrorCode}, Message: {summaryActionResults.Error.Message}");
                        continue;
                    }

                    foreach (ExtractiveSummarizeResult documentResults in summaryActionResults.DocumentsResults)
                    {
                        if (documentResults.HasError)
                        {
                            Console.WriteLine($"  Error!");
                            Console.WriteLine($"  Document error code: {documentResults.Error.ErrorCode}.");
                            Console.WriteLine($"  Message: {documentResults.Error.Message}");
                            stringBuilder.Append($"Error! Document error code: {documentResults.Error.ErrorCode}, Message: {documentResults.Error.Message}");
                            continue;
                        }

                        Console.WriteLine($"  Extracted the following {documentResults.Sentences.Count} sentence(s):");
                        Console.WriteLine();

                        foreach (ExtractiveSummarySentence sentence in documentResults.Sentences)
                        {
                            Console.WriteLine($"  Sentence: {sentence.Text}");
                            Console.WriteLine();
                            stringBuilder.Append(sentence.Text);
                        }
                    }
                }
            }
            _summarizeText = $@"{stringBuilder}";
        }


        private async Task UploadFile(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file != null)
            {
                // Read the file into a stream
                using var stream = file.OpenReadStream();

            }
        }
    }
}
