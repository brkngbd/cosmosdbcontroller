$uri = "http://localhost:38732/api/movies"

#test GET ALL
$movieList = Invoke-WebRequest -Method 'Get' -Uri $uri
Write-Host $movieList

#test Post
$Body = @{
    id = "Terminator"
    Title = "Terminator"
    Year = "1989"
    ImdbRating = "9"
} | ConvertTo-Json

$response1 = Invoke-WebRequest -Method 'Post' -Uri $uri -Body $Body -ContentType "application/json"
Write-Host $response1

$Body = @{
    id = "The Thing"
    Title = "The Thing"
    Year = "1981"
    ImdbRating = "8"
} | ConvertTo-Json

$response2 = Invoke-WebRequest -Method 'Post' -Uri $uri -Body $Body -ContentType "application/json"
Write-Host $response2

$Body = @{
    id = "The Alien"
    Title = "The Alien"
    Year = "1983"
    ImdbRating = "8.9"
} | ConvertTo-Json

$response3 = Invoke-WebRequest -Method 'Post' -Uri $uri -Body $Body -ContentType "application/json"
Write-Host $response3

#test GET ITEM
$id = "The Thing"
$partitionKey = "1981"
$uriFull = "$uri/$id/$partitionKey"
$response4 = Invoke-WebRequest -Method 'Get' -Uri $uriFull
Write-Host $response4

#test GET ITEM must use cache
$id = "The Thing"
$partitionKey = "1981"
$uriFull = "$uri/$id/$partitionKey"
$response4 = Invoke-WebRequest -Method 'Get' -Uri $uriFull
Write-Host $response4

$id = "Terminator"
$partitionKey = "1989"
$uriFull = "$uri/$id/$partitionKey"
$response4 = Invoke-WebRequest -Method 'Get' -Uri $uriFull
Write-Host $response4

#test GET ALL
$movieList2 = Invoke-WebRequest -Method 'Get' -Uri $uri
Write-Host $movieList2

#test PUT
$id = "Terminator"
$partitionKey = "1986"
$Body = @{
    id = "Terminator"
    Title = "Terminator"
    Year = "1989"
    ImdbRating = "1"
} | ConvertTo-Json

$response5 = Invoke-WebRequest -Method 'Put' -Uri "$uri/$id/$partitionKey" -Body $Body -ContentType "application/json"
Write-Host $response5


#test GET ALL
$movieList3 = Invoke-WebRequest -Method 'Get' -Uri $uri
Write-Host $movieList3

#test DELETE
$id = "Terminator"
$partitionKey = "1989"
$response6 = Invoke-WebRequest -Method 'Delete' -Uri "$uri/$id/$partitionKey"
Write-Host $response6
$id = "The Thing"
$partitionKey = "1981"
$response7 = Invoke-WebRequest -Method 'Delete' -Uri "$uri/$id/$partitionKey"
Write-Host $response7
$id = "The Alien"
$partitionKey = "1983"
$response8 = Invoke-WebRequest -Method 'Delete' -Uri "$uri/$id/$partitionKey"
Write-Host $response8

#test GET ALL
$movieList3 = Invoke-WebRequest -Method 'Get' -Uri $uri
Write-Host $movieList3