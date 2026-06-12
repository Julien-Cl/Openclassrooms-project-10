# MedicalApp

Application d'évaluation du risque de diabète de type 2 réalisée en utilisant des microservices, dans le cadre d'un projet Openclassrooms. 


## Comment lancer le projet

Lancement du projet depuis le dossier MedicalApp: docker compose up --build -d

Adresse de l'interface web: http://localhost:7002

Identifiant du compte de test: admin@medicalapp.local

Mot de passe du compte de test: Test123!


## Mesures de green code à mettre en place

- ajout d’un index MongoDB sur PatientId dans la base de données de notes

- utilisation de DTO afin de réduire le volume de données à faire transiter, notamment entre microservices. 
Ex: MicroserviceBackAssessment récupère un patient via GET /patients/{id}, or il n’a besoin que de DateOfBirth et Gender.  

- configurer Ocelot pour mettre en place un rate limiting