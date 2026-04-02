# BTCPay Server Plugin - Asaas PIX

Plugin para BTCPay Server que integra pagamentos PIX através da API do Asaas.

## Funcionalidades

- Pagamentos PIX via API do Asaas
- QR Code dinâmico com expiração
- Código PIX (copia e cola)
- Interface de configuração completa
- Suporte a múltiplas moedas com conversão automática
- Integração nativa com BTCPay Server
- Logging detalhado para debugging

## Configuração

### Pré-requisitos

1. **Conta Asaas**: Crie uma conta em [https://asaas.com](https://asaas.com)
2. **API Key**: Obtenha sua API Key no painel do Asaas
3. **Customer ID**: Tenha um ID de cliente cadastrado no Asaas

### Instalação

1. Copie os arquivos do plugin para a pasta de plugins do BTCPay Server
2. Reinicie o BTCPay Server
3. Configure o plugin em `Settings > Payment Methods`

### Configuração do Plugin

1. Acesse `Settings > Payment Methods`
2. Clique em "PIX (Asaas)"
3. Preencha os campos:
   - **API Key**: Sua API Key do Asaas
   - **Customer ID**: ID do cliente padrão
   - **Habilitar Pagamentos PIX**: Marque esta opção

## Como Funciona

### Fluxo de Pagamento

1. **Cliente escolhe PIX** no checkout
2. **Plugin cria cobrança** via API do Asaas
3. **QR Code é gerado** dinamicamente
4. **Cliente escaneia ou copia** o código PIX
5. **Pagamento é processado** pelo Asaas
6. **Invoice é confirmada** no BTCPay Server

### API Integration

O plugin utiliza os seguintes endpoints da API do Asaas:

- `POST /v3/lean/payments` - Criar cobrança PIX
- `GET /v3/payments/{id}/pixQrCode` - Obter QR Code
- `GET /v3/payments/{id}` - Consultar status

### Conversão de Moeda

O plugin converte automaticamente qualquer moeda suportada pelo BTCPay Server para BRL usando as taxas configuradas.

## Estrutura do Plugin

```
BTCPayServer.Plugins.Asaas/
├── Controllers/
│   └── AsaasPixController.cs
├── PaymentHandler/
│   ├── AsaasPixPaymentMethodHandler.cs
│   ├── AsaasPixPaymentMethodConfig.cs
│   ├── AsaasPixPromptDetails.cs
│   ├── AsaasPixPaymentData.cs
│   └── AsaasPixCheckoutModelExtension.cs
├── Services/
│   ├── AsaasApiService.cs
│   ├── AsaasService.cs
│   └── AsaasDbContextFactory.cs
├── Data/
│   ├── AsaasDbContext.cs
│   └── PluginData.cs
├── Views/
│   ├── AsaasPix/
│   │   └── Configure.cshtml
│   └── Shared/
│       └── AsaasPix/
│           ├── AsaasPixCheckout.cshtml
│           ├── StoreNavExtension.cshtml
│           └── CheckoutPaymentMethodExtension.cshtml
├── Resources/
│   └── asaas-pix.svg
└── BTCPayServer.Plugins.Asaas.csproj
```

## Segurança

- API Keys são armazenadas de forma criptografada
- Dados sensíveis são removidos para não-proprietários
- Logging seguro sem exposição de dados
- Validação de configuração obrigatória

## Suporte

- **Documentação Asaas**: [https://docs.asaas.com](https://docs.asaas.com)
- **BTCPay Server**: [https://btcpayserver.org](https://btcpayserver.org)

## Licença

MIT License - Veja arquivo LICENSE para detalhes.
