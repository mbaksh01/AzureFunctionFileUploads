# AzureFunctionTest

API &rarr; Upload a file directly from Swagger.

Web App &rarr; Simulate a user uploading a file from their browser.

Upload Function &rarr; Simulates a file upload using `Task.Delay()`, completing after 30 seconds.

## Running The Code

1. Download the [Azure Functions CLI Tool](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cportal%2Cv2%2Cbash&pivots=programming-language-csharp#install-the-azure-functions-core-tools).
2. CD into `UploadFunction` and run `func start`.
3. Run the API.
4. Upload a file through Swagger. Swagger should respond after ~3 seconds and there should be logs appearing in the upload function CLI.
5. Use the id from the location header to get the status of the upload.
