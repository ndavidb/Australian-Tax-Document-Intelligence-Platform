# PowerShell script using Invoke-WebRequest (works in older PowerShell)
$functionUrl = "http://localhost:7174/api/documents/upload"
$filePath = "./test-receipts/sample-receipt.pdf"

# Check if file exists
if (Test-Path $filePath) {
    # Get file info
    $fileBytes = [System.IO.File]::ReadAllBytes((Resolve-Path $filePath))
    $fileName = (Get-Item $filePath).Name

    # Create multipart form data
    $boundary = [System.Guid]::NewGuid().ToString()
    $LF = "`r`n"

    $bodyLines = (
    "--$boundary",
    "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"",
    "Content-Type: application/pdf$LF",
    [System.Text.Encoding]::GetEncoding("iso-8859-1").GetString($fileBytes),
    "--$boundary--$LF"
    ) -join $LF

    $response = Invoke-RestMethod -Uri $functionUrl -Method Post -ContentType "multipart/form-data; boundary=$boundary" -Body $bodyLines

    Write-Host "Upload Response:" -ForegroundColor Green
    $response | ConvertTo-Json
} else {
    Write-Host "File not found: $filePath" -ForegroundColor Red
}