FROM registry.redhat.io/rh-sso-7/sso76-openshift-rhel8:7.6-5 as build-env
WORKDIR /source
MAINTAINER acme

MAINTAINER acme

USER root

COPY theme/ $JBOSS_HOME/themes/

RUN chown -R jboss:root $JBOSS_HOME/themes/bcgov-with-local-login

RUN chmod -R 775 $JBOSS_HOME/themes/bcgov-with-local-login
