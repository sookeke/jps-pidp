FROM maven:3.8.2-jdk-11-slim AS builder

COPY ./extensions-7.6 /tmp/
WORKDIR /tmp/
RUN mvn -B clean package --file pom.xml


FROM registry.redhat.io/rh-sso-7/sso76-openshift-rhel8:7.6-5

MAINTAINER acme


USER root

COPY ./extensions-7.6/themes/src/main/resources/theme /opt/eap/themes/


# to deploy the archive, drop it into the `standalone/deployments/` directory and it will be automatically loaded.
COPY --from=builder /tmp/services/target/bcgov-services-1.0.0.jar /opt/eap/standalone/deployments/
COPY configuration/standalone-openshift.xml $JBOSS_HOME/standalone/configuration/standalone-openshift.xml


RUN chown -R jboss:root $JBOSS_HOME/themes/

USER 185