using LLE.Auth.Events;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Security;
using LLE.Pages;
using LLE.ReactFrontend.Events;
using LLE.TypeScript.Events;
using LLE.UiIR;

namespace LLE.Scenarios;

public class ScenarioModule : IModuleLoader
{
    public Task AppStart()
    {
        Eventing.Eventing.Of<DatabaseEvents>().Seeding<IPageRepository>().Concurrent(async pageRepository =>
        {
            var page = new Page()
            {
                Title = "Scenarios",
                Key = "scenarios",
                Url = "/scenarios"
            };
            
            page.From(new VNode("@page/scenarios-page", [], []));
            
            await pageRepository.CreateAsync(page, UserContext.Guest, DataOptions.Bypass);

            return pageRepository;
        });
        
        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(registry =>
        {
            registry.AddAutoImport("./App/Code/Local/Scenarios/Source/web/Pages/Scenarios/index.tsx");
            registry.AddAutoImport("./App/Code/Local/Scenarios/Source/web/Pages/ScenarioSession/index.tsx");
        });
        
        Eventing.Eventing.Of<RolesEventTable>().Ready.Concurrent(async roleRepository =>
        {
            var adminRole = await roleRepository.FindByKeyAsync("admin", UserContext.Guest,  DataOptions.Bypass);
            var userRole = await roleRepository.FindByKeyAsync("user", UserContext.Guest, DataOptions.Bypass);
            
            PolicyEnforcer.SetRule(adminRole.Id, "Scenario_read", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Scenario_read", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "Scenario_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Scenario_create", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(adminRole.Id, "Scenario_update", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Scenario_update", PermissionLevel.OwnedOnly);
            PolicyEnforcer.SetRule(adminRole.Id, "Scenario_delete", PermissionLevel.FullPermission);
            PolicyEnforcer.SetRule(userRole.Id, "Scenario_delete", PermissionLevel.OwnedOnly);
            
            return roleRepository;
        });

        Eventing.Eventing.Of<DatabaseEvents>().Seeding<IScenarioRepository>().Concurrent(async scenarioRepository =>
        {
            async Task CreateScenario(string title, params string[] steps)
            {
                var scenario = new Scenario()
                {
                    Title = title,
                    Steps = string.Join("\n", steps)
                };
                
                await scenarioRepository.CreateAsync(scenario, UserContext.Guest, DataOptions.Bypass); 
            }
            
            // 1. AIRPORT DEBOARDING & BORDER CONTROL
            await CreateScenario(
                "Airport: Deboarding & Border Control",
                "EVENT: The aircraft door opens and you walk into the terminal.",
                "NPC: Good day, please follow the signs for international arrivals and passport control.",
                "USER_ACTION: Greet the staff member and confirm you are heading to passport control.",
                "EVENT: You reach the end of the corridor and arrive at the border control desks.",
                "NPC: Next in line, please. Step forward to desk number four.",
                "USER_ACTION: Walk up to the desk and greet the border control officer.",
                "NPC: May I see your passport and travel documents, please?",
                "USER_ACTION: Hand over your passport and confirm you are giving it to them.",
                "NPC: What is the primary purpose of your visit to our country?",
                "USER_ACTION: State clearly that you are visiting here for a holiday.",
                "NPC: How many days do you intend to stay in the country?",
                "USER_ACTION: State that you plan to stay for exactly ten days.",
                "NPC: Where will you be staying during your time here?",
                "USER_ACTION: Explain that you are staying at a hotel in the city centre.",
                "EVENT: The officer stamps your passport and hands it back.",
                "NPC: Everything looks perfect. Enjoy your stay here.",
                "USER_ACTION: Say thank you, take your passport, and walk toward the main exit."
            );

            // 2. TAXIING
            await CreateScenario(
                "Taxi: Travelling to Your Destination",
                "EVENT: You walk out of the terminal to the designated taxi rank.",
                "NPC: Hello there, where can I drive you today?",
                "USER_ACTION: Greet the driver and give them the name of your destination hotel.",
                "NPC: Sure thing, that is about a twenty-minute drive. Put your luggage in the boot.",
                "USER_ACTION: Thank them and state that you are putting your bag in the back.",
                "EVENT: You get into the back seat and the driver starts the engine.",
                "USER_ACTION: Ask the driver how much the journey will cost approximately.",
                "NPC: It usually runs around thirty euros depending on the traffic traffic.",
                "USER_ACTION: Acknowledge the price and tell them that is completely fine.",
                "EVENT: The taxi approaches a major highway intersection.",
                "NPC: Would you prefer me to take the scenic route or the faster toll road?",
                "USER_ACTION: Request the faster route to save time.",
                "EVENT: The taxi pulls up outside the front doors of your hotel.",
                "NPC: Here we are, right outside the main lobby. The total comes to thirty-two euros.",
                "USER_ACTION: Tell the driver you will pay by credit card.",
                "NPC: Card machine is ready. Just tap right there.",
                "USER_ACTION: Confirm the payment went through, say goodbye, and exit the car."
            );

            // 3. BUSING
            await CreateScenario(
                "Bus: Using Public Transport",
                "EVENT: You stand at a busy city bus shelter looking at a route map.",
                "USER_ACTION: Ask a nearby passenger if bus line line ten goes to the history museum.",
                "NPC: Yes, line ten goes straight there. It arrives every fifteen minutes.",
                "USER_ACTION: Thank them for confirming the route info.",
                "EVENT: A large blue bus pulls up to the platform and opens its front door.",
                "NPC: Good afternoon. Please step inside.",
                "USER_ACTION: Greet the bus driver and ask for one single ticket to the museum.",
                "NPC: That will be two euros and fifty cents, please.",
                "USER_ACTION: State that you are handing over the exact cash amount.",
                "NPC: Thank you, here is your paper ticket. Take a seat anywhere.",
                "USER_ACTION: Thank the driver and walk down the aisle to find an open seat.",
                "EVENT: The bus drives through the city and multiple stops pass by.",
                "USER_ACTION: Ask the person sitting next to you if the next stop is the museum.",
                "NPC: No, the museum is two stops away. I can tell you when we get there.",
                "USER_ACTION: Express your gratitude for their help.",
                "EVENT: The bus slows down again and the passenger taps your shoulder.",
                "NPC: This is your stop right here. The museum is just across the street.",
                "USER_ACTION: Thank them warmly and walk toward the exit door."
            );

            // 4. TRAIN TRAVEL
            await CreateScenario(
                "Train Travel: Booking and Riding",
                "EVENT: You walk into the central train station concourse.",
                "USER_ACTION: Approach the ticket counter and ask for a ticket to the capital city.",
                "NPC: Would you like a one-way ticket or a return ticket?",
                "USER_ACTION: State that you would like a one-way ticket, please.",
                "NPC: The next express train departs from platform five in ten minutes.",
                "USER_ACTION: Ask if there is a first-class seat option available.",
                "NPC: Yes, first class has open seats. That will be forty-five euros total.",
                "USER_ACTION: Hand over your payment card and confirm.",
                "NPC: Payment confirmed. Here is your ticket. Hurry to platform five.",
                "USER_ACTION: Thank them and ask which direction platform five is.",
                "NPC: Turn right at the newspaper kiosk and go straight down the stairs.",
                "USER_ACTION: Say thank you and move quickly toward the platform.",
                "EVENT: You board the train just before the doors slide closed.",
                "NPC: Tickets please, have your travel passes ready for validation.",
                "USER_ACTION: Show your ticket to the conductor who is standing in the aisle."
            );

            // 5. RESTAURANT
            await CreateScenario(
                "Restaurant: Ordering dinner",
                "EVENT: You step inside a cosy traditional restaurant.",
                "NPC: Good evening, welcome. Do you have a table reservation tonight?",
                "USER_ACTION: Say no and ask if they have a free table for one person.",
                "NPC: Right this way. Follow me to this table by the window.",
                "USER_ACTION: Thank them and sit down in the chair.",
                "EVENT: The waiter places a physical menu on the table surface.",
                "NPC: Here is the menu. Can I start you off with something to drink?",
                "USER_ACTION: Order a glass of red wine and some tap water.",
                "NPC: Excellent choice. I will bring the drinks and take your food order shortly.",
                "EVENT: The waiter returns with your glasses and holds a small notepad.",
                "NPC: Are you ready to order your main meal or do you need more time?",
                "USER_ACTION: Order the grilled steak with a side of roasted potatoes.",
                "NPC: How would you like that cooked? Rare, medium, or well done?",
                "USER_ACTION: Specify that you would like the steak cooked medium, please.",
                "EVENT: You finish your entire meal and clear your plate.",
                "USER_ACTION: Catch the waiter's eye and ask for the bill, please.",
                "NPC: Right away. Did you enjoy everything tonight?",
                "USER_ACTION: Compliment the food quality and confirm everything was excellent."
            );

            // 6. HOTEL
            await CreateScenario(
                "Hotel: Check-In & Finding Your Room",
                "EVENT: You enter the bright hotel lobby holding your heavy suitcases.",
                "NPC: Welcome to the Grand Plaza Hotel. How can I assist you today?",
                "USER_ACTION: Greet the receptionist and say you want to check in.",
                "NPC: Certainly. What is the last name on the room reservation?",
                "USER_ACTION: Spell out your last name clearly for them.",
                "NPC: Found it. I just need a valid photo ID to complete the check-in process.",
                "USER_ACTION: Hand over your ID card and confirm you are doing so.",
                "NPC: Perfect. You are in room number three hundred and five, on the third floor.",
                "USER_ACTION: Ask the receptionist what time breakfast is served in the morning.",
                "NPC: Breakfast is open from seven AM until ten AM in the main dining hall.",
                "USER_ACTION: Thank them and ask where the guest elevators are located.",
                "NPC: The lifts are right behind that stone pillar to your left.",
                "USER_ACTION: Say thank you and wish them a pleasant evening.",
                "EVENT: You take the elevator to the third floor and walk down the corridor.",
                "USER_ACTION: Ask a passing cleaning staff member to point out room three hundred and five.",
                "NPC: It is just around the corner on the right side, sir.",
                "USER_ACTION: Thank them for pointing it out."
            );

            // 7. SHOPPING
            await CreateScenario(
                "Shopping: Finding Items in a Store",
                "EVENT: You walk into a large multi-storey department store.",
                "USER_ACTION: Approach an employee near the entrance and ask where the clothing department is.",
                "NPC: Men's fashion is on the second floor, and women's fashion is on the third floor.",
                "USER_ACTION: Thank them and head up the escalator to find what you need.",
                "EVENT: You are looking around the clothing racks but cannot find your size.",
                "USER_ACTION: Ask a nearby sales assistant if they have this jacket in a medium size.",
                "NPC: Let me check the stock system. Yes, we have one more left in the back storage room.",
                "USER_ACTION: Say thank you and tell them you will gladly wait here.",
                "EVENT: The assistant returns holding the medium jacket.",
                "NPC: Here it is. Would you like to try it on before buying?",
                "USER_ACTION: Say yes and ask where the fitting rooms are located.",
                "NPC: The changing rooms are right at the back of the floor, next to the mirrors.",
                "USER_ACTION: Thank them for their help and walk toward the cubicles.",
                "EVENT: You decide you like the item and walk up to the checkout cash desk.",
                "NPC: Hello, did you find everything you were looking for today?",
                "USER_ACTION: Say yes and explain that you would like to buy this item now."
            );

            return scenarioRepository;
        });
        
        Features.LoadFeatures();
        
        return Noop();
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}