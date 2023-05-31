# BulkyBook wholesale and e-commerce bookstore
## A large scale Dotnet 8 Web project by Gerard Heuvelman

Made by completing the following course on dotnetMastery.com: https://dotnetmastery.com/Home/Details?courseId=9
A live version of this Ecommerce website can be found at https://bulky.azurewebsites.net/

# Local installation 
- Clone this repository as a new Dotnet 8 solution
- Install SQL Server (version 19.0.2 and up)
- Create a Database called "Bulky"
- Copy the connection string
- In this solution, in the "BulkyWeb" project, paste the connection string in Apsettings.json, under ConnectionStrings => DefaultConnection
- Find the Package manager Console. In Visual Studio 2022, you can find it by clicking Tools => Package Manager => PAckage Manager Console
- in The PM Console, type "update-database" and press enter.
- Run the project.

# Optional
- To add Stripe digital payment capabilities to the project, add your own public and private Stripe key to appsettings.json in the bulkytweb project before running it. It you do not, you will only be able to complete payment using a "company" account (approved for delayed payment).