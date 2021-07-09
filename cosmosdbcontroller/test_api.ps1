$uri = "http://localhost:38732/api/movies"

#test GET ALL
$movieList = Invoke-WebRequest -Method 'Get' -Uri $uri
Write-Host $movieList

#test Post
$Body = @{
    id = "Terminator.1986"
    Title = "Terminator"
    ImdbRating = "9"
} | ConvertTo-Json

$response1 = Invoke-WebRequest -Method 'Post' -Uri $uri -Body $Body -ContentType "application/json"
Write-Host $response1

$Body = @{
    id = "The Thing.1981"
    Title = "The Thing"
    ImdbRating = "8"
} | ConvertTo-Json

$response2 = Invoke-WebRequest -Method 'Post' -Uri $uri -Body $Body -ContentType "application/json"
Write-Host $response2

$Body = @{
    id = "The Alien.1983"
    Title = "The Alien"
    ImdbRating = "8.9"
} | ConvertTo-Json

$response3 = Invoke-WebRequest -Method 'Post' -Uri $uri -Body $Body -ContentType "application/json"
Write-Host $response3

#test GET ITEM
$id = "The Thing.1981"
$response4 = Invoke-WebRequest -Method 'Get' -Uri "$uri/$id"
Write-Host $response4

#test GET ALL
$movieList2 = Invoke-WebRequest -Method 'Get' -Uri $uri
Write-Host $movieList2

#test PUT
$id = "Terminator.1986"
$Body = @{
    id = "Terminator.1986"
    Title = "Terminator"
    ImdbRating = "1"
} | ConvertTo-Json

$response5 = Invoke-WebRequest -Method 'Put' -Uri "$uri/$id" -Body $Body -ContentType "application/json"
Write-Host $response5

#test GET ALL
$movieList3 = Invoke-WebRequest -Method 'Get' -Uri $uri
Write-Host $movieList3

#test DELETE
$id = "Terminator.1986"
$response6 = Invoke-WebRequest -Method 'Delete' -Uri "$uri/$id"
Write-Host $response6
$id = "The Thing.1981"
$response7 = Invoke-WebRequest -Method 'Delete' -Uri "$uri/$id"
Write-Host $response7
$id = "The Alien.1983"
$response8 = Invoke-WebRequest -Method 'Delete' -Uri "$uri/$id"
Write-Host $response8