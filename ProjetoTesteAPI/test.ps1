param(
    [string]$BaseUrl = "http://localhost:5005"
)

function Test-Api {
    param($Url)
    try {
        $r = Invoke-RestMethod -Uri $Url -Method Get -TimeoutSec 5
        Write-Output "OK GET $Url"
        $r | ConvertTo-Json -Depth 10
    } catch {
        Write-Error "FAILED GET $Url - $($_.Exception.Message)"
        return $null
    }
}

Write-Output "BaseUrl: $BaseUrl"

# 1) GET all
Write-Output "\n[1] GET /api/produto"
Test-Api "$BaseUrl/api/produto"

# 2) POST new product
Write-Output "\n[2] POST /api/produto"
$body = @{ nome='TesteScript'; preco=9.9; quantidade=10 } | ConvertTo-Json
try {
    $post = Invoke-RestMethod -Uri "$BaseUrl/api/produto" -Method Post -Body $body -ContentType 'application/json' -TimeoutSec 10
    Write-Output "POST OK: id=$($post.id)"
    $post | ConvertTo-Json -Depth 10
} catch {
    Write-Error "POST FAILED - $($_.Exception.Message)"
    exit 1
}

# 3) GET all to confirm
Write-Output "\n[3] GET all after POST"
Test-Api "$BaseUrl/api/produto"

# 4) PUT update
$id = $post.id
Write-Output "\n[4] PUT /api/produto/$id"
$body2 = @{ id=$id; nome='TesteScript Atualizado'; preco=11.5; quantidade=20 } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/produto/$id" -Method Put -Body $body2 -ContentType 'application/json' -TimeoutSec 10
    Write-Output "PUT OK for id=$id"
    Test-Api "$BaseUrl/api/produto/$id"
} catch {
    Write-Error "PUT FAILED - $($_.Exception.Message)"
}

# 5) DELETE
Write-Output "\n[5] DELETE /api/produto/$id"
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/produto/$id" -Method Delete -TimeoutSec 10
    Write-Output "DELETE OK for id=$id"
} catch {
    Write-Error "DELETE FAILED - $($_.Exception.Message)"
}

# 6) Final GET
Write-Output "\n[6] Final GET /api/produto"
Test-Api "$BaseUrl/api/produto"

Write-Output "\nScript finished."