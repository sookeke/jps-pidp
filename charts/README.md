## Helm Charts

These Helm charts permit the simple deployment of the services relating to the PIDP application.

Helm charts should be installed/upgraded with the correct values for the given environment.

In order to deploy the helm charts you would need to be logged into the correct OpenShift project.

e.g.

```
oc login ....

oc project <project_name>

helm list 

helm upgrade --install --values=..\deploy\<env>_values.yaml pidp pidp
```
