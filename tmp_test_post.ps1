$r = Invoke-WebRequest 'http://localhost:5224/Usuario/Cadastrar' -UseBasicParsing -SessionVariable sess
$content = $r.Content
$token = ''
if ($content -match 'name="__RequestVerificationToken"[^>]*value="([^"]+)"') { $token=$matches[1] }
Write-Output "TOKEN:$token"
$body = @{ '__RequestVerificationToken' = $token; 'TipoUsuario'='Morador'; 'Email'='testmorador+ps@local.test'; 'Senha'='senha123'; 'NomeCompleto'='Teste Morador'; 'Apartamento'='APT 101'; 'Telefone'='11999999999' }
$resp = Invoke-WebRequest 'http://localhost:5224/Usuario/Cadastrar' -Method Post -Body $body -WebSession $sess -MaximumRedirection 0 -ErrorAction SilentlyContinue
if ($resp -and $resp.StatusCode) {
    Write-Output "Status:$($resp.StatusCode)"
    if ($resp.Headers['Location']) { Write-Output "Location:$($resp.Headers['Location'])" } else { Write-Output 'No Location header' }
} else {
    Write-Output 'No response object'
    if ($resp -and $resp.Content) {
        $contentSnippet = $resp.Content.Substring(0,[Math]::Min(500,$resp.Content.Length))
        Write-Output $contentSnippet
    }
}
