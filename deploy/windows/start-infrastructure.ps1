[CmdletBinding()]
param(
    [string]$RabbitMqServiceName = 'RabbitMQ',
    [string]$RedisServiceName
)

$ErrorActionPreference = 'Stop'

function Resolve-WindowsService {
    param(
        [Parameter(Mandatory = $true)][string]$Label,
        [string]$RequestedName,
        [Parameter(Mandatory = $true)][string]$DiscoveryPattern
    )

    $services = @(Get-Service)
    if (-not [string]::IsNullOrWhiteSpace($RequestedName)) {
        $matches = @($services | Where-Object {
            $_.Name -eq $RequestedName -or $_.DisplayName -eq $RequestedName
        })
    }
    else {
        $matches = @($services | Where-Object {
            $_.Name -match $DiscoveryPattern -or $_.DisplayName -match $DiscoveryPattern
        })
    }

    if ($matches.Count -eq 0) {
        if ($Label -eq 'Redis-compatible server') {
            throw "No Redis/Memurai Windows service was found. Install and configure Memurai or another Redis-compatible Windows service, or pass its service name with -RedisServiceName."
        }

        throw "The $Label Windows service '$RequestedName' was not found. Check the installed service name and pass it with the matching script parameter."
    }

    if ($matches.Count -gt 1) {
        $names = ($matches | ForEach-Object { $_.Name }) -join ', '
        throw "More than one $Label service matched ($names). Specify the exact service name."
    }

    return $matches[0]
}

function Start-InfrastructureService {
    param([Parameter(Mandatory = $true)]$Service)

    $current = Get-Service -Name $Service.Name
    if ($current.Status -ne [System.ServiceProcess.ServiceControllerStatus]::Running) {
        Write-Host "Starting $($current.DisplayName)..."
        Start-Service -Name $current.Name
        $controller = Get-Service -Name $current.Name
        $controller.WaitForStatus(
            [System.ServiceProcess.ServiceControllerStatus]::Running,
            [TimeSpan]::FromSeconds(60))
    }
    else {
        Write-Host "$($current.DisplayName) is already running."
    }
}

function Test-LocalTcpPort {
    param([Parameter(Mandatory = $true)][int]$Port)

    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $pending = $client.BeginConnect('127.0.0.1', $Port, $null, $null)
        if (-not $pending.AsyncWaitHandle.WaitOne(1500, $false)) {
            return $false
        }

        $client.EndConnect($pending)
        return $client.Connected
    }
    catch {
        return $false
    }
    finally {
        $client.Dispose()
    }
}

try {
    $rabbitMq = Resolve-WindowsService `
        -Label 'RabbitMQ' `
        -RequestedName $RabbitMqServiceName `
        -DiscoveryPattern '(?i)rabbitmq'
    $redis = Resolve-WindowsService `
        -Label 'Redis-compatible server' `
        -RequestedName $RedisServiceName `
        -DiscoveryPattern '(?i)(memurai|redis)'

    Start-InfrastructureService -Service $rabbitMq
    Start-InfrastructureService -Service $redis

    foreach ($endpoint in @(
        [pscustomobject]@{ Name = 'RabbitMQ'; Port = 5672 },
        [pscustomobject]@{ Name = 'Redis-compatible server'; Port = 6379 }
    )) {
        if (-not (Test-LocalTcpPort -Port $endpoint.Port)) {
            throw "$($endpoint.Name) service is running, but localhost:$($endpoint.Port) is not accepting connections. Check its configuration and logs."
        }
        Write-Host "$($endpoint.Name) is accepting connections on localhost:$($endpoint.Port)."
    }

    Write-Host 'Infrastructure services are ready.' -ForegroundColor Green
}
catch {
    Write-Error $_
    exit 1
}
