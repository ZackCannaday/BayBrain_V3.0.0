using BayBrain.Models;
using System.Collections.Generic;

namespace BayBrain.Data
{
    /// <summary>
    /// In-memory service catalog — replace with JSON/DB load as needed.
    /// </summary>
    public static class ServiceDatabase
    {
        public static List<ServiceItem> GetAll() => new()
        {
            // ─── OIL & FLUIDS ────────────────────────────────────────────
            new ServiceItem
            {
                Id = "oil_change",
                Name = "Oil Change",
                Category = "Oil & Fluids",
                ShortDescription = "Replace engine oil and filter to keep your engine running clean and cool.",
                DetailedDescription = "Engine oil lubricates all moving metal parts, reduces friction, absorbs heat, and carries away contaminants. Over time, oil breaks down, becomes acidic, and loses viscosity — leading to sludge and accelerated wear. Modern full-synthetic oils last 7,500–10,000 miles; conventional 3,000–5,000 miles.",
                WhyItMatters = "Skipping oil changes is the #1 cause of premature engine failure. A fresh oil change costs $60–$100; an engine replacement costs $4,000–$10,000+.",
                Interval = "Every 5,000–7,500 mi (full synthetic) or 3,000–5,000 mi (conventional)",
                SkipConsequence = "Oil sludge, increased engine wear, potential engine seizure.",
                BaseUrgency = 8,
                PriceRange = "$59–$109",
                EstimatedMinutes = 30,
                Tags = new() { "oil", "lube", "filter", "synthetic", "conventional", "engine", "5w30", "0w20" }
            },
            new ServiceItem
            {
                Id = "transmission_fluid",
                Name = "Transmission Fluid Service",
                Category = "Oil & Fluids",
                ShortDescription = "Drain and replace the fluid that keeps your transmission shifting smoothly.",
                DetailedDescription = "Transmission fluid lubricates gears, clutches, and seals. It also acts as hydraulic fluid in automatic transmissions. Degraded fluid loses viscosity and can cause harsh shifting, slipping, and overheating. Most manufacturers recommend service every 30,000–60,000 miles.",
                WhyItMatters = "Transmission replacement averages $3,000–$7,000. A fluid service at $150–$250 can prevent that.",
                Interval = "Every 30,000–60,000 miles or per manufacturer recommendation",
                SkipConsequence = "Harsh or delayed shifting, overheating transmission, premature clutch pack failure.",
                BaseUrgency = 6,
                PriceRange = "$149–$249",
                EstimatedMinutes = 60,
                Tags = new() { "transmission", "fluid", "atf", "cvt", "gear", "shifting", "slipping" }
            },
            new ServiceItem
            {
                Id = "coolant_flush",
                Name = "Coolant / Antifreeze Flush",
                Category = "Oil & Fluids",
                ShortDescription = "Replace the coolant that prevents your engine from overheating or freezing.",
                DetailedDescription = "Coolant (antifreeze) regulates engine temperature and prevents corrosion inside the cooling system. Over time, pH drops, corrosion inhibitors deplete, and water pump seals can be damaged. Old coolant can cause scaling and blockages in the radiator.",
                WhyItMatters = "Overheating is a top cause of catastrophic engine damage. A head gasket failure alone runs $1,500–$4,000.",
                Interval = "Every 30,000 miles or every 2–5 years depending on coolant type",
                SkipConsequence = "Corrosion in cooling system, thermostat failure, water pump damage, overheating.",
                BaseUrgency = 6,
                PriceRange = "$99–$149",
                EstimatedMinutes = 45,
                Tags = new() { "coolant", "antifreeze", "radiator", "flush", "overheat", "temperature", "water pump" }
            },
            new ServiceItem
            {
                Id = "brake_fluid",
                Name = "Brake Fluid Exchange",
                Category = "Oil & Fluids",
                ShortDescription = "Replace the hydraulic fluid that activates your brakes every time you press the pedal.",
                DetailedDescription = "Brake fluid is hygroscopic — it absorbs water from the atmosphere over time. As water content rises, the fluid's boiling point drops (vapor lock risk) and it promotes internal corrosion of calipers, lines, and the ABS modulator. DOT 3/4 should be replaced every 2 years or 30,000 miles.",
                WhyItMatters = "Vapor lock under heavy braking can cause complete brake failure. ABS modulator replacement costs $500–$1,500.",
                Interval = "Every 2 years or 30,000 miles",
                SkipConsequence = "Spongy brake pedal, vapor lock, internal corrosion, brake failure risk.",
                BaseUrgency = 7,
                PriceRange = "$89–$129",
                EstimatedMinutes = 30,
                Tags = new() { "brake", "fluid", "DOT3", "DOT4", "hydraulic", "ABS", "spongy", "pedal" }
            },
            new ServiceItem
            {
                Id = "power_steering_fluid",
                Name = "Power Steering Fluid Service",
                Category = "Oil & Fluids",
                ShortDescription = "Flush the fluid that makes steering effortless.",
                DetailedDescription = "Hydraulic power steering systems rely on clean fluid to transmit force. Contaminated fluid can damage the power steering pump and rack seals, leading to leaks and hard steering. Electric power steering (EPS) vehicles do not require this service.",
                WhyItMatters = "Power steering pump replacement: $200–$600. Rack & pinion: $1,000–$2,500.",
                Interval = "Every 30,000–50,000 miles or if fluid appears dark/gritty",
                SkipConsequence = "Noisy pump, hard steering, seal degradation, pump failure.",
                BaseUrgency = 4,
                PriceRange = "$79–$119",
                EstimatedMinutes = 30,
                Tags = new() { "power steering", "fluid", "pump", "rack", "hard steering", "whine" }
            },

            // ─── BRAKES ──────────────────────────────────────────────────
            new ServiceItem
            {
                Id = "brake_pads",
                Name = "Brake Pad Replacement",
                Category = "Brakes",
                ShortDescription = "Replace worn brake pads before they damage your rotors and compromise stopping power.",
                DetailedDescription = "Brake pads press against rotors to create friction and slow the vehicle. Most pads have wear indicators that squeal when they reach ~3mm. Once metal-on-metal contact occurs, rotors are scored and must be replaced too, dramatically increasing repair cost.",
                WhyItMatters = "Pads alone: $150–$300/axle. Waiting until metal-on-metal: add $200–$400 for rotors. More critically — stopping distance increases significantly.",
                Interval = "Every 25,000–70,000 miles depending on driving style",
                SkipConsequence = "Reduced stopping power, rotor damage, safety risk.",
                BaseUrgency = 9,
                PriceRange = "$149–$299 per axle",
                EstimatedMinutes = 60,
                Tags = new() { "brake", "pads", "rotors", "squealing", "grinding", "stopping", "safety", "caliper" }
            },
            new ServiceItem
            {
                Id = "rotor_resurface",
                Name = "Brake Rotor Replacement",
                Category = "Brakes",
                ShortDescription = "Replace warped or worn rotors to eliminate vibration and restore braking performance.",
                DetailedDescription = "Rotors can warp from heat cycles or become too thin to safely resurface. Warped rotors cause pedal pulsation and vibration during braking. Modern thin-cast rotors are typically replaced rather than resurfaced.",
                WhyItMatters = "Pulsating brake pedal indicates compromised stopping ability. Warped rotors extend stopping distances.",
                Interval = "Every 50,000–70,000 miles or when thickness falls below minimum spec",
                SkipConsequence = "Vibration, longer stopping distance, caliper damage.",
                BaseUrgency = 8,
                PriceRange = "$200–$450 per axle (with pads)",
                EstimatedMinutes = 90,
                Tags = new() { "rotor", "disc", "warp", "vibration", "pulsation", "brake", "shudder" }
            },

            // ─── TIRES & ALIGNMENT ───────────────────────────────────────
            new ServiceItem
            {
                Id = "tire_rotation",
                Name = "Tire Rotation",
                Category = "Tires & Alignment",
                ShortDescription = "Move tires to different positions so they wear evenly and last longer.",
                DetailedDescription = "Front tires wear faster due to steering forces. Rotating tires every 5,000–7,500 miles equalizes tread wear across all four tires, extending tire life by up to 50% and maintaining balanced handling.",
                WhyItMatters = "Skipping rotation can halve tire life. A set of tires costs $400–$1,200. Rotation is $20–$50.",
                Interval = "Every 5,000–7,500 miles (align with oil changes)",
                SkipConsequence = "Uneven tread wear, premature tire replacement, handling imbalance.",
                BaseUrgency = 5,
                PriceRange = "$19–$49",
                EstimatedMinutes = 30,
                Tags = new() { "tire", "rotation", "wear", "tread", "balance", "alignment" }
            },
            new ServiceItem
            {
                Id = "wheel_alignment",
                Name = "Wheel Alignment",
                Category = "Tires & Alignment",
                ShortDescription = "Adjust wheel angles so your car drives straight and tires wear evenly.",
                DetailedDescription = "Alignment adjusts camber, caster, and toe angles to manufacturer specs. Misalignment from potholes, curb strikes, or worn suspension components causes rapid tire wear on one edge and can pull the vehicle to one side.",
                WhyItMatters = "Misalignment can destroy a set of new tires in 10,000 miles. Alignment service is $80–$130.",
                Interval = "Every 12,000 miles or after suspension work / tire purchase",
                SkipConsequence = "Rapid edge wear on tires, pulling/drifting, premature tire failure.",
                BaseUrgency = 6,
                PriceRange = "$79–$129",
                EstimatedMinutes = 60,
                Tags = new() { "alignment", "pull", "drift", "camber", "toe", "caster", "steering", "suspension" }
            },
            new ServiceItem
            {
                Id = "tire_balance",
                Name = "Tire Balancing",
                Category = "Tires & Alignment",
                ShortDescription = "Balance weights on wheels to eliminate vibration at highway speeds.",
                DetailedDescription = "Unbalanced tires cause vibration felt through the steering wheel or seat at certain speeds. Over time this vibration accelerates wear on wheel bearings and suspension components.",
                WhyItMatters = "Wheel bearing replacement: $250–$500. Balancing: $15–$25 per tire.",
                Interval = "Every 12,000 miles or when vibration is noticed",
                SkipConsequence = "Vibration, uneven tread wear, bearing and suspension wear.",
                BaseUrgency = 5,
                PriceRange = "$60–$100 (set of 4)",
                EstimatedMinutes = 30,
                Tags = new() { "balance", "vibration", "shimmy", "wheel", "highway", "shake" }
            },

            // ─── FILTERS ─────────────────────────────────────────────────
            new ServiceItem
            {
                Id = "cabin_air_filter",
                Name = "Cabin Air Filter",
                Category = "Filters",
                ShortDescription = "Replace the filter that cleans the air you breathe inside the cabin.",
                DetailedDescription = "The cabin air filter traps dust, pollen, mold spores, and pollutants before they enter the passenger compartment through the HVAC system. A clogged filter reduces airflow, strains the blower motor, and allows allergens through.",
                WhyItMatters = "Air quality directly affects driver alertness and passenger health. Blower motor replacement: $200–$400.",
                Interval = "Every 15,000–25,000 miles or 1 year",
                SkipConsequence = "Poor cabin air quality, reduced A/C and heat performance, blower motor strain.",
                BaseUrgency = 4,
                PriceRange = "$29–$59",
                EstimatedMinutes = 15,
                Tags = new() { "cabin", "air filter", "pollen", "HVAC", "allergies", "smell", "musty", "A/C", "heat" }
            },
            new ServiceItem
            {
                Id = "engine_air_filter",
                Name = "Engine Air Filter",
                Category = "Filters",
                ShortDescription = "Replace the filter protecting your engine from dirt and debris.",
                DetailedDescription = "The engine air filter prevents abrasive particles from entering the intake and causing cylinder wall wear. A restricted filter leans out the air/fuel mixture, reduces power, and increases fuel consumption.",
                WhyItMatters = "A dirty filter can reduce fuel economy by up to 10%. Replacing it costs $20–$50 and takes minutes.",
                Interval = "Every 15,000–30,000 miles",
                SkipConsequence = "Reduced power, poor fuel economy, potential engine contamination.",
                BaseUrgency = 5,
                PriceRange = "$29–$69",
                EstimatedMinutes = 15,
                Tags = new() { "air filter", "engine", "intake", "fuel economy", "mpg", "power", "dirty" }
            },

            // ─── BATTERY & ELECTRICAL ────────────────────────────────────
            new ServiceItem
            {
                Id = "battery_test",
                Name = "Battery Test & Replacement",
                Category = "Battery & Electrical",
                ShortDescription = "Test your battery's health and replace it before it leaves you stranded.",
                DetailedDescription = "Lead-acid batteries degrade over 3–5 years. Cold cranking amps (CCA) drop with age, making cold starts difficult. A bad battery can also damage the alternator as it works harder to compensate. Battery tests take 5 minutes with a conductance tester.",
                WhyItMatters = "Being stranded costs time and potentially a tow ($100–$200). Alternator damage adds $300–$700.",
                Interval = "Test every year after 3 years; replace every 3–5 years",
                SkipConsequence = "No-start condition, stranded vehicle, potential alternator damage.",
                BaseUrgency = 7,
                PriceRange = "$149–$229 (replacement)",
                EstimatedMinutes = 30,
                Tags = new() { "battery", "dead", "no start", "crank", "alternator", "electrical", "slow crank", "clicking" }
            },
            new ServiceItem
            {
                Id = "spark_plugs",
                Name = "Spark Plug Replacement",
                Category = "Battery & Electrical",
                ShortDescription = "Replace worn spark plugs to restore engine performance and fuel economy.",
                DetailedDescription = "Spark plugs ignite the air/fuel mixture in each cylinder. Worn plugs misfire, reducing power, increasing fuel consumption, and potentially damaging the catalytic converter ($800–$2,500) from unburned fuel passing through.",
                WhyItMatters = "A catalytic converter damaged by misfires costs many times more than a plug service.",
                Interval = "Every 30,000 miles (copper/platinum) or 60,000–100,000 miles (iridium)",
                SkipConsequence = "Misfires, rough idle, poor fuel economy, catalytic converter damage.",
                BaseUrgency = 7,
                PriceRange = "$149–$299",
                EstimatedMinutes = 60,
                Tags = new() { "spark plug", "misfire", "rough idle", "fuel economy", "ignition", "cylinder", "catalytic" }
            },

            // ─── SUSPENSION & STEERING ───────────────────────────────────
            new ServiceItem
            {
                Id = "shocks_struts",
                Name = "Shocks & Struts Replacement",
                Category = "Suspension & Steering",
                ShortDescription = "Replace worn shocks or struts to restore ride comfort and vehicle control.",
                DetailedDescription = "Shocks and struts dampen road vibration and keep tires in contact with the road. Worn units cause excessive body roll, nose-dive under braking, bounce over bumps, and increased stopping distance. The 'bounce test' (push down on a corner — if it bounces more than once, shocks are likely worn).",
                WhyItMatters = "Worn struts increase stopping distance by up to 20% and accelerate tire and suspension wear.",
                Interval = "Every 50,000–100,000 miles or when bounce/handling issues appear",
                SkipConsequence = "Poor handling, increased stopping distance, tire cupping, unsafe control.",
                BaseUrgency = 7,
                PriceRange = "$299–$799 per axle",
                EstimatedMinutes = 120,
                Tags = new() { "shocks", "struts", "bounce", "handling", "suspension", "ride", "control", "cupping" }
            },

            // ─── SCHEDULED MAINTENANCE ───────────────────────────────────
            new ServiceItem
            {
                Id = "30k_service",
                Name = "30,000-Mile Service",
                Category = "Scheduled Maintenance",
                ShortDescription = "Comprehensive inspection and fluid/filter service at the 30K milestone.",
                DetailedDescription = "Typically includes: engine air filter, cabin air filter, spark plugs (if due), tire rotation, brake inspection, fluid level top-offs, multi-point vehicle inspection, and transmission fluid service. Catches developing issues before they become expensive repairs.",
                WhyItMatters = "Milestone services maintain manufacturer warranty compliance and catch ~70% of developing issues early.",
                Interval = "At 30,000-mile odometer milestone",
                SkipConsequence = "Missed wear items, potential warranty concerns, compounding maintenance debt.",
                BaseUrgency = 7,
                PriceRange = "$199–$399",
                EstimatedMinutes = 120,
                Tags = new() { "30k", "30000", "service", "maintenance", "inspection", "scheduled" }
            },
            new ServiceItem
            {
                Id = "60k_service",
                Name = "60,000-Mile Service",
                Category = "Scheduled Maintenance",
                ShortDescription = "Major milestone service covering all wear items at the 60K mark.",
                DetailedDescription = "Includes all 30K service items plus: timing belt inspection (if applicable), coolant flush, brake fluid exchange, fuel system cleaning, serpentine belt inspection, and full suspension/steering check.",
                WhyItMatters = "The 60K service is the most important milestone — many systems hit wear thresholds simultaneously.",
                Interval = "At 60,000-mile odometer milestone",
                SkipConsequence = "Multiple deferred maintenance items compounding into expensive failures.",
                BaseUrgency = 8,
                PriceRange = "$399–$699",
                EstimatedMinutes = 180,
                Tags = new() { "60k", "60000", "service", "major", "maintenance", "timing belt" }
            },
            new ServiceItem
            {
                Id = "timing_belt",
                Name = "Timing Belt Replacement",
                Category = "Scheduled Maintenance",
                ShortDescription = "Replace the belt that synchronizes your engine's valves and pistons — before it snaps.",
                DetailedDescription = "Interference engines (most modern engines) will suffer catastrophic valve/piston collision if the timing belt breaks. The belt shows no external warning before failure. Water pump is typically replaced at the same time since it shares the belt path.",
                WhyItMatters = "A snapped timing belt = bent valves, damaged pistons = $3,000–$10,000 engine repair or replacement. Belt + water pump service: $500–$1,200.",
                Interval = "Every 60,000–105,000 miles per manufacturer spec",
                SkipConsequence = "Catastrophic engine destruction if belt snaps on an interference engine.",
                BaseUrgency = 10,
                PriceRange = "$499–$1,199",
                EstimatedMinutes = 240,
                Tags = new() { "timing belt", "timing chain", "water pump", "interference", "snapped", "engine failure" }
            },
        };
    }
}
