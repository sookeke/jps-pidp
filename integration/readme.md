# JPIDP INTERGRATION STACK

- Kafka Cluster with SASL/SSL + OAUTH
- Kafka Zookeeper
- Kafka Connect with Postgres, SQL Server Connectors
- Kafka Debezium Connectors with SSL
- Kafka UI with SSO
- Debezium UI
- Apicurio Schema Registry with SSO Authentication and Authorization
- Kafka Stream i.e Kafka KSQL Server


##Pre-requisite

- Openshift Red Hat Integration - AMQ Stream operator
- Red Hat Inetgration - Camel K Operator
- Red Hat Service Registry operator
- Red Hat Knative operator
- KEDA operator
- KEDA http-ad-on Operator

```bash
install kafka ui
helm repo add kafka-ui https://provectus.github.io/kafka-ui
	
helm install kafka-ui kafka-ui/kafka-ui --set envs.config.KAFKA_CLUSTERS_0_SCHEMAREGISTRY=http://dems-apicurioregistry-kafkasql-service.5b7aa5-dev.svc.cluster.local:8080/apis/ccompat/v6 --set envs.config.AUTH_TYPE=OAUTH2 --set envs.config.SPRING_SECURITY_OAUTH2_CLIENT_REGISTRATION_AUTH0_CLIENTID=kafka-ui --set envs.config.SPRING_SECURITY_OAUTH2_CLIENT_REGISTRATION_AUTH0_CLIENTSECRET=122223333333333333 --set envs.config.SPRING_SECURITY_OAUTH2_CLIENT_REGISTRATION_AUTH0_SCOPE=openid --set envs.config.SPRING_SECURITY_OAUTH2_CLIENT_PROVIDER_AUTH0_ISSUER_URI=https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth/realms/DEMSPOC --set envs.config.KAFKA_CLUSTERS_0_NAME=dems-cluster --set envs.config.KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS=dems-cluster-kafka-bootstrap:9092

helm install --set kafka.enabled=false --set kafka.bootstrapServers=SSL://dems-cluster-kafka-bootstrap:9093 --set schema-registry.enabled=false --set schema-registry.url=http://dems-apicurioregistry-kafkasql-service.5b7aa5-dev.svc.cluster.local:8080 --set kafka-connect.enabled=false --set kafka-connect.url=jpidp-debezium-connect-api.5b7aa5-dev.svc.cluster.local ktool rhcharts/ksqldb

```

```bash
helm install ksqlserver --set kafka.enabled=false --set kafka.bootstrapServers=SSL://dems-cluster-kafka-bootstrap:9093 --set schema-registry.enabled=false --set schema-registry.url=http://dems-apicurioregistry-kafkasql-service.5b7aa5-dev.svc.cluster.local:8080 --set kafka-connect.enabled=false --set kafka-connect.url=jpidp-debezium-connect-api.5b7aa5-dev.svc.cluster.local --set ksql.headless=false .\cp-ksql-server\ --debug
```
## Securing Kafka Cluster 
- Encrypt data in transist
- Encrypt Data in rest using Harshicop Vault
- Secure client with proven Identity with a Centralized Authrorization server
- Validate JWT token issuer
- Validate scope and audience

```bash
      - name: tls
        port: 9093
        type: internal
        tls: true
      - authentication:
          userNameClaim: preferred_username
          clientId: kafka-broker
          enableOauthBearer: true
          validIssuerUri: >-
            https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth/realms/DEMSPOC
          maxSecondsWithoutReauthentication: 3600
          tlsTrustedCertificates:
            - certificate: keycloak.crt
              secretName: ca-keycloak
          type: oauth
          checkAudience: true
          clientSecret:
            key: clientSecret
            secretName: dems-kafka-cluster
          introspectionEndpointUri: >-
            https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth/realms/DEMSPOC/protocol/openid-connect/token/introspect
          tokenEndpointUri: >-
            https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth/realms/DEMSPOC/protocol/openid-connect/token
        configuration:
          bootstrap:
            host: dems-cluster-5b7aa5-test.apps.silver.devops.gov.bc.ca
          createBootstrapService: true
        name: external
        port: 9094
        tls: true
        type: route
```

<img align="center" width="1110" src="../docs/Solution Architecture.drawio.png">

###Microservices
- jpidp service
- justin user manager service
- edt service adapter
- Email and paper mail template generator sevice
- Notification Service (email and paper mail)
