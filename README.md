# Prion Database Helper Core


## Instalando Pacote com NUGET
Abra o `Console do Gerenciador de Pacotes` pelo Visual Studio e execute o seguinte comando

```c
Install-Package Prion.DbHelper -Version 1.0.0
```

## Atualizar Pacote no NUGET
Altere o assembly do projeto, principalmente a **versão** do novo pacote. Execute o seguinte comando para gerar um novo pacote `.nupkg`. 

```c
nuget pack
```
> É necessário ter o executável do nuget em algum diretório acessível pelo PATH do sistema

Acesse o site do nuget.org e efetue o upload do arquivo `.nupkg` gerado