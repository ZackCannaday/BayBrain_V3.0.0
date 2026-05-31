using BayBrain.Models;
using System.Collections.Generic;

namespace BayBrain.Data
{
    public static class QuizDatabase
    {
        public static List<QuizQuestion> GetAll() => new()
        {
            new QuizQuestion
            {
                Id = "q1",
                Question = "What is the PRIMARY purpose of engine oil?",
                Options = new() { "Cool the engine only", "Lubricate moving parts and reduce friction", "Clean the fuel injectors", "Regulate ignition timing" },
                CorrectIndex = 1,
                Explanation = "Engine oil lubricates all moving metal parts, reducing friction and heat. It also carries away contaminants and acts as a coolant supplement.",
                RelatedServiceId = "oil_change",
                Category = "Oil & Fluids",
                DifficultyLevel = 1
            },
            new QuizQuestion
            {
                Id = "q2",
                Question = "A customer's oil hasn't been changed in 12,000 miles with conventional oil. What urgency level is this?",
                Options = new() { "Low — they have plenty of time", "Moderate — mention it as a suggestion", "High — recommend immediate service", "Critical — engine may be damaged" },
                CorrectIndex = 2,
                Explanation = "Conventional oil typically needs changing every 3,000–5,000 miles. At 12,000 miles, the oil is severely degraded and engine wear is actively occurring. This is a high urgency situation.",
                RelatedServiceId = "oil_change",
                Category = "Oil & Fluids",
                DifficultyLevel = 2
            },
            new QuizQuestion
            {
                Id = "q3",
                Question = "When brake pads reach metal-on-metal contact, what additional repair is typically required?",
                Options = new() { "Only new brake fluid", "Rotor replacement in addition to pads", "Only caliper adjustment", "ABS sensor replacement" },
                CorrectIndex = 1,
                Explanation = "When pads wear through, bare metal grinds against the rotor, scoring it deeply. Once scored beyond minimum thickness spec, rotors must be replaced — significantly increasing repair cost.",
                RelatedServiceId = "brake_pads",
                Category = "Brakes",
                DifficultyLevel = 1
            },
            new QuizQuestion
            {
                Id = "q4",
                Question = "What does the term 'interference engine' mean in relation to timing belt service?",
                Options = new() { "The engine is louder than normal", "Valve and piston paths overlap — a broken belt causes catastrophic damage", "The timing belt also drives the alternator", "The engine requires premium fuel" },
                CorrectIndex = 1,
                Explanation = "In an interference engine, valves and pistons occupy the same space at different times. If the timing belt breaks, they collide — bending valves and potentially destroying pistons. This is why timing belt replacement before failure is critical.",
                RelatedServiceId = "timing_belt",
                Category = "Scheduled Maintenance",
                DifficultyLevel = 3
            },
            new QuizQuestion
            {
                Id = "q5",
                Question = "Why is brake fluid considered a 'hygroscopic' fluid, and why does that matter?",
                Options = new() { "It glows in the dark for leak detection", "It absorbs water from the atmosphere, lowering its boiling point", "It becomes more viscous over time", "It is only used in ABS systems" },
                CorrectIndex = 1,
                Explanation = "Brake fluid absorbs moisture from air. As water content rises, the boiling point drops. Under hard braking, fluid can boil and vaporize, causing the pedal to go to the floor — a condition called vapor lock.",
                RelatedServiceId = "brake_fluid",
                Category = "Brakes",
                DifficultyLevel = 2
            },
            new QuizQuestion
            {
                Id = "q6",
                Question = "A customer says their car 'pulls to the right' and they notice uneven tire wear. What service is most likely needed?",
                Options = new() { "Tire rotation only", "Oil change", "Wheel alignment", "Spark plug replacement" },
                CorrectIndex = 2,
                Explanation = "Pulling to one side combined with uneven tire wear are classic indicators of wheel misalignment. Without alignment correction, tires will continue to wear rapidly on one edge.",
                RelatedServiceId = "wheel_alignment",
                Category = "Tires & Alignment",
                DifficultyLevel = 1
            },
            new QuizQuestion
            {
                Id = "q7",
                Question = "How does a clogged cabin air filter affect the HVAC system beyond air quality?",
                Options = new() { "It has no mechanical effect", "It strains the blower motor, potentially causing premature failure", "It increases fuel consumption", "It affects the transmission" },
                CorrectIndex = 1,
                Explanation = "A severely clogged cabin filter restricts airflow, causing the blower motor to work harder against increased resistance. This generates excess heat in the motor and can lead to premature failure ($200–$400 repair).",
                RelatedServiceId = "cabin_air_filter",
                Category = "Filters",
                DifficultyLevel = 2
            },
            new QuizQuestion
            {
                Id = "q8",
                Question = "What is the 'bounce test' used to evaluate?",
                Options = new() { "Tire pressure", "Shock absorber and strut condition", "Battery health", "Brake pad thickness" },
                CorrectIndex = 1,
                Explanation = "Push down firmly on a vehicle corner and release. If it bounces more than once, the shock or strut is likely worn. Properly functioning dampers should settle in one controlled motion.",
                RelatedServiceId = "shocks_struts",
                Category = "Suspension & Steering",
                DifficultyLevel = 1
            },
            new QuizQuestion
            {
                Id = "q9",
                Question = "A customer's battery is 4 years old in Georgia summer heat. No symptoms yet. What should you recommend?",
                Options = new() { "Do nothing — wait for symptoms", "Test the battery and discuss proactive replacement", "Replace immediately without testing", "Only check the alternator" },
                CorrectIndex = 1,
                Explanation = "Heat dramatically accelerates battery degradation. A 4-year-old battery in a hot climate is statistically near end of life. Testing costs nothing and gives the customer data to make an informed decision — avoiding a no-start situation.",
                RelatedServiceId = "battery_test",
                Category = "Battery & Electrical",
                DifficultyLevel = 2
            },
            new QuizQuestion
            {
                Id = "q10",
                Question = "How can worn spark plugs damage the catalytic converter?",
                Options = new() { "They increase exhaust backpressure", "Misfires send unburned fuel into the exhaust, overheating and melting the catalyst", "They reduce oil pressure to the converter", "They have no effect on the catalytic converter" },
                CorrectIndex = 1,
                Explanation = "Misfiring cylinders push raw unburned fuel into the exhaust stream. The catalytic converter tries to combust this fuel, generating extreme heat that can melt the internal honeycomb structure — resulting in a $800–$2,500 repair.",
                RelatedServiceId = "spark_plugs",
                Category = "Battery & Electrical",
                DifficultyLevel = 3
            },
            new QuizQuestion
            {
                Id = "q11",
                Question = "What is the best way to explain tire rotation value to a cost-conscious customer?",
                Options = new() { "Tell them it's required by law", "Explain that $30 now can save $400–$1,200 in early tire replacement", "Say it makes the car safer immediately", "Avoid bringing it up if they seem reluctant" },
                CorrectIndex = 1,
                Explanation = "Cost-conscious customers respond to ROI framing. Tire rotation equalizes wear and can extend tire life by 50%. A $30 service protecting a $600+ tire investment is a compelling, concrete value proposition.",
                RelatedServiceId = "tire_rotation",
                Category = "Tires & Alignment",
                DifficultyLevel = 1
            },
            new QuizQuestion
            {
                Id = "q12",
                Question = "Coolant that appears orange or has rusty particles indicates what problem?",
                Options = new() { "Normal color variation — nothing to worry about", "Severe internal corrosion — immediate flush required", "The coolant is still good", "A problem only with the overflow tank" },
                CorrectIndex = 1,
                Explanation = "Orange-tinged or rusty coolant means corrosion inhibitors have been fully depleted and oxidation is occurring inside the cooling system. Continuing to run this coolant accelerates damage to the water pump, thermostat, and radiator.",
                RelatedServiceId = "coolant_flush",
                Category = "Oil & Fluids",
                DifficultyLevel = 2
            },
        };
    }
}
