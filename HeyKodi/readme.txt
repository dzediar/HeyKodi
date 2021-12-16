x Créer un moteur d'écoute de la reco vocale
x Rendre ce moteur "robuste"
x Rapatrier les éléments de KodiRPC dans HeyKodi
x Intégrer la zcomp, utiliser dans la zcomp le nugget mvvm de microsoft
x Implémenter le pattern MVVM
x Lister les commandes kodi à gérer
x Créer une classe de configuration à sérialiser / désérialiser
x Créer la vue de la classe de config
x Créer une vue principale sexy et animée
x Mettre des icones à la place du bouton de fermeture, ajouter un bouton config
x Revoir l'exécution des commandes et la gestion des états dans le recognizer
x Afficher la commande reconnue
x Transparence du bouton fermer de l'infobulle
x Utiliser un message pour afficher la config
x Mettre un timer lorsque codi est activé => désactivation
x Ajouter une commande pour afficher la config de heykodi
x Ajouter la commande pour annuler l'activation de codi
x Réduire codi lorsque il repasse en attente
x Utiliser des ICommand mvvm
x Trouver une icone d'application
non Gérer le tray icon
x Config volume
x Créer des classes commandes kodi avec un execute()
x Gérer toutes les commandes listées
x Mettre les sons en ressources
x Créer un installer complet
x Gérer la synthèse vocale pour les commandes à paramètres
x Mettre les sources sous github
x Créer une doc succinte dans le wiki
x Faire en sorte que l'appli se lance au démarrage de windows
x Tester toutes les commandes sur le mediacenter en utilisant l'installer
x Vérifier le wrapptext dans l'infobulle pour les longs messages
x Essayer d'ajouter la grammaire des titres de film car la reconnaissance vocale est nulle, récupérer les musiques et les séries tv, les auteurs aussi
x Tester l'utilisation d'un mot de passe
x Vérifier au lancement de heykodi si il est déjà lancé
x Regarder les warnings
Gérer le souci de l'ouverture de heykodi avant kodi+
Créer une première release

C'est très mal codé, nettoyer le code, tout mettre à sa place dans les bonnes couches
Choix des langues de reconnaissance et de synthèse vocale
Commande avec comme paramètre un pourcentage (volume, ...)
Localiser l'appli
Crypter le mdp kodi
Afficher l'exception uniquement lorsqu' le mode debug est activé, donner un maxwidth au content du speechballon, en permettant le wrapping du texte, trouver un rouge correct pour l'exceptionviewer
Tester tester tester... (sur le médiacenter)
Créer un installer
Mettre une déco sur la fenêtre principale de launchmeup pour éviter le scaling windows
La réduction et l'activation ne se font plus en mode commande directe

Commandes : 
	éteindre
	stop
	play
	suivant (chapitre)
	mute
	doucement (son)
	fort (son)
	films
	séries
	favori (ajout)
	favoris (afficher)
	accueil
	go (sélection)
	ping (mettre un timeout faible)
	sous-titres => avec param
	ejecter (lecteur optique)
	dormir
	éteindre
	redémarrer
