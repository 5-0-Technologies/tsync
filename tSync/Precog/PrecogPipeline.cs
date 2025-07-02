using Azure.Messaging.EventHubs.Consumer;
using Blake3;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SDK;
using SDK.Contracts.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using tSync.Core.Localization;
using tSync.Filters;
using tSync.Precog.Filters;
using tSync.Precog.Models;
using tSync.Precog.Options;
using tSync.TwinzoApi;
using tUtils.Filters;
using tUtils.Filters.Input;
using tUtils.Filters.InputOutput;

namespace tSync.Precog
{
    public class PrecogPipeline : Pipeline
    {
        private readonly PrecogPipelineOptions opt;
        private readonly ILogger logger;

        static readonly string[] wordArray = new string[] { "Coding", "Fix", "Marque", "Wilted", "Gibbon", "It", "Trier", "Tooled", "Ye", "Bison", "Reeked", "Fed", "Pappy", "Poland", "Terry", "Scat", "Racial", "Mynahs", "Yellow", "Rough", "Pipe", "Banked", "Hansom", "Bratty", "Fridge", "Wavily", "Twist", "Fluke", "Insert", "Clear", "Stood", "Parcel", "Abuse", "Count", "Fixate", "Risk", "Want", "Crimea", "Ration", "Amman", "Fuzz", "Mule", "Iris", "Page", "Acrid", "Heavy", "Satin", "Weedy", "Kerned", "Why", "Injoke", "Haggle", "Niggle", "Slate", "Hood", "Accord", "Fugue", "Adjure", "Biker", "Auntie", "Vestal", "Pin", "Story", "Ballet", "Snuff", "Get", "Crust", "Ulcer", "Wary", "Tshirt", "Hamlet", "Bagful", "Mien", "Siding", "Lolly", "Plump", "Abut", "Vulcan", "Here", "Lap", "Billed", "Eyeful", "Sorrow", "Lugged", "Ripper", "Dagger", "Launch", "Wallop", "Reify", "Built", "Eraser", "Hardup", "Yule", "Aster", "Rot", "Four", "Lyre", "Demon", "Cavern", "Nicer", "Vain", "Vicar", "Nitric", "Lazy", "Pear", "Agiler", "Farmer", "Deity", "Gourd", "Bawdy", "Pose", "Mucosa", "Remap", "Flex", "Grange", "Pistol", "Glide", "Eluted", "Slaver", "Gander", "Arena", "Fee", "Endues", "Sarge", "Legato", "Peaked", "Bypass", "Soh", "Den", "Allure", "Busy", "Rubble", "Sicily", "Bing", "Drench", "Rink", "Rite", "Spire", "Mash", "Gape", "Tin", "Wily", "Fried", "Prism", "Nestle", "Sync", "Amber", "Cent", "Maiden", "Aegina", "Unholy", "Deuce", "Combed", "Matte", "Dulled", "Sawing", "Pursue", "Lift", "Onuses", "Pinkie", "Spry", "Cherry", "Organ", "Libido", "Replay", "Camber", "Heal", "Pippin", "Stung", "Resent", "Slug", "Spotty", "Room", "Anemic", "Finer", "Dock", "Bland", "Braise", "Bodkin", "Ginger", "Dutch", "Hearer", "Future", "Latch", "Foaled", "Epoch", "Dilly", "Giro", "Sagely", "Lie", "Almost", "Hover", "Moss", "Sermon", "Typing", "Poke", "Medic", "Warmly", "Ninth", "Mouth", "Whimsy", "Phobia", "Salaam", "Axe", "Gopher", "Rooted", "Mixes", "Nose", "Lacy", "Rabbit", "Tone", "Hooped", "Size", "Fasten", "Nearer", "Unworn", "Hunt", "Boring", "Preyed", "Twirl", "Lanky", "Hiring", "Recoil", "Vague", "King", "Joke", "Fab", "Unmask", "Versus", "Hippo", "Thane", "Smutty", "Aimer", "Sirius", "Floaty", "Azure", "Bigamy", "Seedy", "Vino", "Coke", "Airbus", "Harass", "Twiggy", "Exempt", "Billow", "Fig", "Limo", "Plain", "Citing", "Abash", "Falter", "Moult", "Inner", "Passe", "Sneeze", "Studio", "Rested", "Ceres", "Ammo", "Exhume", "Bean", "Tally", "Hack", "Remiss", "Idly", "Notify", "Snide", "Biased", "Bern", "Tight", "Seem", "Code", "Usual", "Lent", "Leave", "Voter", "Sunbed", "Amazon", "These", "Friday", "Feuded", "Regina", "Christ", "Summit", "Grader", "Tanner", "Gusset", "Bedder", "Mew", "Motion", "Hauler", "Armada", "Waver", "Inrush", "Owe", "Beatup", "Scape", "Shame", "Savvy", "Apace", "Rapist", "Strum", "Fumble", "Risky", "Tiny", "Tench", "Nugget", "Arming", "Sepia", "Troll", "Pellet", "Pelt", "Firkin", "Mayday", "Wile", "Bauble", "Holily", "Treat", "Endow", "Slow", "Faller", "Reopen", "Ana", "Bap", "Mucous", "Pop", "Caftan", "Acumen", "Primer", "Tibia", "Dermis", "Dear", "Peaky", "Emigre", "Coming", "Bird", "Fib", "Idiocy", "Puppet", "Unpick", "Laxer", "Prank", "Jinks", "Wagged", "Pretty", "Chilli", "Fourth", "Egging", "Tannin", "Sud", "Flirt", "Least", "Siren", "Hither", "Sitcom", "Wadi", "Coupe", "Worsen", "Broach", "Vest", "Ousted", "Smelly", "Chide", "Slinky", "Fedup", "Bowl", "Apollo", "Guy", "Butt", "Candid", "Solder", "Aura", "Fife", "Zinc", "Fester", "Shrew", "Rice", "Induct", "Boxer", "Nitwit", "Thresh", "Leggy", "Quarto", "Task", "Ruth", "Wench", "Delve", "Torn", "Joyful", "Berk", "Tent", "Trust", "Reflex", "Enrich", "Smog", "Futon", "Escape", "Stoker", "Jehad", "Uproar", "Ampler", "Caught", "Glare", "Lawn", "Letter", "Cuing", "Else", "Consul", "Six", "Told", "Dazzle", "Monaco", "Damask", "Stably", "Sam", "Brew", "Peeler", "Heed", "Titan", "Imago", "Batik", "Carted", "Rugged", "Access", "Fobbed", "Tropic", "Suave", "Trick", "Tragic", "Cordon", "Jock", "Burner", "Modem", "Afar", "Allot", "Slunk", "Deader", "Baddy", "Finely", "Ocelot", "Grumps", "Backed", "Starry", "Tetra", "Spilt", "Inland", "Baulky", "Bathe", "Limpet", "Waken", "Nosing", "Sexist", "Damn", "Purvey", "Lawmen", "Penal", "Scour", "Mealy", "Over", "Pallet", "Peril", "Ticked", "Peer", "Cove", "Helm", "Navy", "Dinky", "Demean", "Dealer", "Farce", "Circa", "Lute", "Ingot", "Sugary", "Valour", "Simply", "Crafty", "Milan", "Prime", "Glove", "Terror", "Dud", "Issue", "Inkpot", "Convoy", "Fervid", "Flan", "Diode", "Chow", "Advice", "Gilds", "Arable", "Tootle", "Priory", "Belay", "Hide", "Tawdry", "Wished", "Cart", "For", "Hall", "Raster", "Cocked", "Ritual", "Rote", "Mazy", "Vastly", "Mumps", "Repast", "Waited", "Rem", "Purple", "Patio", "Admen", "Lamp", "Goofed", "Chary", "Depth", "Flue", "Dip", "Adit", "Ill", "Imbue", "Medial", "Wept", "Wearer", "Diktat", "Ibis", "Smug", "Deduce", "Baking", "Congo", "Holier", "Active", "Salute", "Musk", "Cost", "Talkie", "Resize", "Cabmen", "Mater", "Adore", "Rimmed", "Alias", "Wok", "Fracas", "Him", "Crotch", "Then", "Physio", "Masher", "Charge", "Coda", "Humble", "Cavort", "Wham", "Sit", "Apple", "Womb", "Bombed", "Invite", "Exist", "Nicest", "Disuse", "Unclad", "Erst", "Stance", "Shrunk", "Sties", "Carver", "Few", "Tincan", "Spying", "Kaiser", "Foxier", "Fungi", "Dumbly", "Ready", "Dim", "Hazard", "Sedate", "Wacky", "Gigolo", "Furry", "Larynx", "Cocain", "Spit", "Prolix", "Dare", "Henry", "Chance", "Mill", "Ticker", "Hating", "Avenge", "Yard", "Sprain", "Kiwi", "Lout", "Gnash", "Rafter", "Flaky", "Oracle", "Locked", "Jolly", "Wad", "Lessor", "Uterus", "Rusty", "Radix", "Aware", "Coiled", "Grew", "Maria", "Aptest", "Inlaid", "Gene", "Pat", "Ethos", "Oxford", "Smear", "Honey", "Dabbed", "Decay", "Crony", "Mr", "Glean", "Frolic", "Elite", "Arabic", "Shop", "Cornet", "Salami", "Laches", "Moat", "Vier", "Tureen", "Lining", "Brave", "Dumped", "Eg", "Payer", "Grunge", "Mild", "Sect", "Score", "Morn", "Safety", "Syria", "Dab", "Alarm", "Lotto", "Rowdy", "Tobago", "Plaid", "Top", "Heat", "Video", "Jockey", "Effete", "Samba", "Secure", "Raid", "David", "Foiled", "Panned", "Plural", "Lob", "Spongy", "Bleak", "Rococo", "Apogee", "Amine", "Dinghy", "Speed", "Byline", "Skewed", "Lurch", "Gelled", "Mug", "Deny", "Hawked", "Crime", "Midway", "Elegy", "Toasty", "Golden", "Hemp", "Faucet", "Pike", "Yaw", "Briny", "Loan", "Ample", "Clap", "Buddy", "Behave", "Aright", "Chest", "Guppy", "Hint", "Lazily", "Slave", "Extra", "Inter", "Inn", "Cliche", "Blithe", "Groan", "Hitch", "Affix", "Fakery", "Dipped", "Egret", "Raft", "Acre", "Arbour", "Weary", "Sanded", "Side", "Swerve", "Eldest", "Drowsy", "Ruff", "Upbeat", "Oxen", "Erode", "Junket", "Coil", "Nato", "Club", "Dan", "Join", "Cruder", "Sprout", "Verb", "Wodge", "Tamest", "Unfelt", "Hiccup", "Ibex", "Wooing", "Fickle", "Mafia", "Ultra", "Snoek", "Nasty", "Stony", "Dryish", "Keying", "Turin", "Moist", "Witter", "Stasis", "Drawer", "Ratify", "Brahms", "Jersey", "Comma", "Caesar", "Swains", "Woolly", "Bruise", "Cyclic", "Gale", "Ilk", "Panic", "Weeper", "Meagre", "Dauber", "Linnet", "Cohort", "Ski", "Haiku", "Adios", "Notice", "Liken", "Bump", "Prison", "Graze", "Spicy", "Quartz", "Copies", "Eking", "Laurel", "Ensure", "Pander", "Coeval", "Thirst", "Dux", "Kingly", "Sen", "Loaded", "Offer", "Cad", "Octet", "Leaden", "Limp", "Cymbal", "Event", "Follow", "Fjord", "Strike", "Dreary", "Affect", "Undone", "Delict", "Acme", "Venous", "Marten", "Open", "Bonn", "Bronco", "Biota", "Defied", "Squirm", "Unseen", "Flint", "Logjam", "Capri", "Income", "Bothy", "Islet", "Pit", "Curare", "Motile", "Athens", "Log", "Debut", "Zaire", "Kicked", "Dolmen", "Roof", "Modal", "Stylus", "Pixie", "Bedpan", "Florid", "Run", "Ear", "Toitoi", "Volga", "Quagga", "Rarest", "Bedbug", "Player", "Meteor", "Faster", "Borer", "Wince", "Dour", "Savant", "Level", "Diatom", "Outrun", "Plea", "Gluing", "Foetus", "Dating", "Floret", "Bright", "Doyen", "Etch", "Silted", "Sacred", "Rudder", "Mauled", "Fishy", "Facer", "Thing", "Portal", "Rusted", "Whelp", "Shark", "Cipher", "Corded", "Thumb", "Roster", "Hooded", "Raj", "Buoyed", "Wilier", "Oiled", "Egoist", "Slower", "Hotbed", "Coin", "Wealth", "Most", "Temple", "Hedge", "Bled", "Mogul", "Lull", "Brake", "Sewer", "Jive", "Ague", "Gimlet", "Tyke", "Cinch", "Hemen", "Vise", "Amiss", "Strict", "Gulp", "Nature", "Saxon", "Kidney", "Ignite", "Plague", "Purdah", "Oboist", "Tinpot", "Mascot", "Viral", "Starve", "Alter", "Flora", "Genus", "Girder", "Causal", "Bedsit", "Recess", "Up", "Craft", "Dispel", "Dial", "Frumps", "Stone", "Goodly", "Saucy", "Groin", "Balsam", "Bonier", "Punky", "Gaging", "Stock", "Hold", "Forbad", "Buy", "Sucker", "Bring", "Brainy", "Lumbar", "Tail", "Older", "Grope", "Oast", "Girth", "Helium", "Genius", "Artier", "Airy", "Bull", "Raspy", "Doomed", "Vane", "Gnat", "Dingle", "Stale", "Edgier", "Tango", "Unseal", "Farad", "Pause", "Apache", "Accra", "Nomad", "Callus", "Vegan", "Windy", "Rectum", "Jackal", "Sixth", "Zap", "Rook", "Rewind", "Misted", "Info", "West", "Sheaf", "Booted", "Ibsen", "Zoomed", "Helix", "Nadir", "Their", "Dashes", "Brunt", "Amok", "Goitre", "Plight", "Brit", "Foyer", "Soler", "Forded", "Sanely", "Uboats", "Wiring", "Own", "Group", "Latin", "Render", "Unity", "Unique", "Feeder", "Cicada", "Burble", "Tanker", "Abler", "Boater", "Seat", "Spiral", "Dumper", "Spree", "Dourly", "Boast", "Silent", "Dehorn", "Outlet", "Ported", "Retro", "Random", "Bulk", "John", "Home", "Occupy", "Embank", "Growl", "Lounge", "Woody", "Soapy", "Tomboy", "Bola", "Dagama", "Zoning", "Rhesus", "Toupee", "Ablest", "Relay", "Gate", "Canned", "Stupid", "Bask", "Plenty", "Frieze", "Talk", "Hock", "Abel", "Shoe", "Coerce", "Loaf", "Paint", "Waking", "Upload", "Stack", "Aghast", "Wear", "Fiddle", "Mole", "Bream", "Opium", "Dipper", "Simple", "Nobody", "Spouse", "Rioted", "Fitful", "Stiff", "Remove", "Dodger", "Remus", "Hark", "Filer", "Mammy", "Telly", "Swing", "Eire", "Defeat", "Frisky", "Reign", "Lagos", "Sprat", "Riding", "Antic", "Bazaar", "Caking", "Pleb", "Waster", "Hazier", "Airgun", "Planar", "Greet", "Glance", "Trend", "Ok", "Pout", "Cheque", "Jalopy", "Infer", "Grant", "Intent", "Zest", "Essay", "Strip", "Burr", "Item", "Equal", "Bagman", "Lobby", "Hung", "Kent", "Alt", "Greed", "Amino", "Waxing", "Pushed", "Beep", "Bitchy", "Sagest", "Aby", "Doting", "Hail", "Search", "Men", "Hag", "Cob", "Pool", "Missus", "Indaba", "Flap", "Tahiti", "Pass", "Nap", "Pundit", "Palmy", "Paper", "Paidup", "Eulogy", "Thames", "Gemini", "Medal", "Veined", "Dotage", "Queasy", "Locate", "Appeal", "Cooled", "Cabman", "Justly", "Ceylon", "Burped", "Cubic", "Uke", "Triad", "Birch", "Reword", "Sawyer", "Shard", "Evoke", "Bigot", "Cloaca", "Loamy", "Biped", "Waggle", "Broke", "Annul", "Caveat", "Doing", "Curly", "Physic", "Brine", "Kisses", "Mind", "Dozy", "Movie", "Wishes", "Crept", "Outfit", "Shrub", "August", "Meet", "Flank", "Plenum", "Evil", "Booby", "String", "Scifi", "Help", "Schwa", "Cruise", "Nail", "Tinder", "Labia", "Drip", "Wimple", "Omen", "Scoff", "Luanda", "Bestir", "Oink", "Outlaw", "Julep", "Accede", "Tenth", "Dusty", "Scab", "Safest", "Flying", "Cult", "Grass", "Florin", "Kernel", "Fuller", "Cation", "This", "Hub", "Barre", "Weak", "Frivol", "Tamp", "Hunted", "Brasil", "Patois", "Wiry", "Sole", "Talmud", "Chat", "Lunch", "Aha", "Been", "Grit", "Shrine", "Rapt", "Magi", "Tumult", "Monday", "Impale", "Albino", "Frost", "Smegma", "Corset", "Seller", "Caucus", "Fondle", "Paltry", "Pouch", "Began", "Candy", "Unhand", "Bug", "Elk", "Umbra", "To", "Ladle", "Gamut", "Patten", "Cystic", "Victim", "Elicit", "Fleet", "Robot", "Point", "Vocal", "Excite", "Flute", "Mellow", "Result", "Lancer", "Dog", "Lase", "Dong", "Verge", "Scream", "Dying", "Ebbing", "Itches", "Slake", "Fish", "Stagey", "Picnic", "Thief", "Scan", "Extend", "Tallow", "Unwrap", "Roofer", "Morsel", "Zonal", "Filing", "Fibers", "Plan", "Axon", "Grimly", "Aloft", "Jonah", "Clause", "Weapon", "Black", "Gawky", "Airier", "Hanged", "Plumb", "Play", "Peep", "Ranked", "Amply", "Call", "Trying", "Drown", "Soso", "Reap", "Handel", "Mullet", "Amuck", "Care", "Tilled", "Sick", "Choosy", "Damply", "Fuming", "Bellow", "Rune", "Rebuff", "Grubby", "Clef", "Partly", "Treble", "Bowel", "Junker", "Boot", "Memory", "Doe", "Zither", "Prove", "Shies", "Howl", "Killed", "Whoosh", "Hazel", "Towel", "Ergot", "Recede", "Soul", "Blob", "Strand", "Sled", "Rodeo", "Vouch", "June", "Maser", "Butted", "French", "Fuse", "Death", "Donned", "Drape", "Net", "Lineup", "Joy", "Beery", "Tomato", "Sworn", "Gym", "Bawl", "Must", "Report", "Tarsus", "Duplex", "Walrus", "Yorker", "Oddly", "Dirty", "Sienna", "Fete", "Spark", "Sleep", "Aspire", "Floor", "Well", "Smell", "Ruck", "Wicket", "Ararat", "Sward", "Sad", "Wavy", "Laddie", "Arthur", "Scrub", "Ageing", "Best", "Fresco", "Mean", "Gushed", "Salem", "Poplar", "Geneva", "Distal", "Climb", "Oompah", "Algae", "Wrap", "Hunch", "Isle", "Deeper", "Outer", "Lack", "Idlest", "Naomi", "Needle", "Shield", "Worded", "Goatee", "Danzig", "Pizza", "Dish", "Trite", "Rialto", "Insole", "Boston", "Prick", "Hurrah", "Bin", "Popped", "Inject", "Lilies", "Gusty", "Icebox", "Eke", "Brace", "Utter", "Nanny", "Casino", "He", "Strive", "Mad", "Hallow", "Styli", "Sextet", "Python", "Seal", "Dafter", "Earth", "Punnet", "Move", "Swap", "Tiding", "Skit", "Stroll", "Swirl", "Sonora", "Unbent", "Briber", "Shod", "Assume", "Petted", "Fancy", "Dammed", "Bookie", "Riper", "Berry", "Neared", "Roman", "Ozone", "Circus", "Slovak", "Shelve", "Brig", "Thinly", "Lady", "Spud", "Pulled", "Mecca", "Stand", "Innate", "Chaff", "Hulk", "Twit", "Pearly", "Genome", "Naked", "Sully", "Shandy", "Ganged", "Cohere", "Ware", "Bissau", "Marina", "Dam", "Blow", "Seldom", "Fudge", "Norway", "Mitre", "Daniel", "Nurse", "Cursor", "Tidied", "Bare", "Prefix", "Shabby", "Arguer", "Whisky", "Mine", "Jihad", "Samoa", "Noon", "Beet", "Turnip", "Spew", "Etches", "Dinner", "Reek", "Optic", "Ampule", "Bumper", "Husk", "Ocean", "Gain", "Urge", "Loft", "Corny", "Diana", "Guile", "Manna", "Reborn", "Binge", "Eunuch", "Matins", "Toning", "Smile", "Primus", "Huh", "Quid", "Coined", "Tenor", "Quaver", "Forgot", "Decoy", "Twitch", "Next", "Puffin", "Flight", "Lest", "Fetal", "Lash", "Social", "Apian", "Full", "Giggly", "Moan", "Host", "Better", "Tigris", "Tarry", "Delhi", "Repose", "Blench", "Repaid", "Stunt", "Pole", "Clove", "Pouffe", "Peace", "Bogged", "Hockey", "Chroma", "Motive", "Bark", "Froggy", "Week", "Since", "Blade", "Dipole", "Abide", "Cutest", "Basely", "Pliers", "Cougar", "Carnal", "Emit", "Mocker", "Kinder", "Arched", "Editor", "Finite", "Ethnic", "Harked", "Peaty", "Tout", "Chewy", "Gilt", "Wilt", "Ext", "Allied", "Scruff", "Busier", "Churn", "Seemly", "Grovel", "Denies", "Beck", "Overt", "Skein", "Purser", "Quake", "Upward", "Lifted", "Work", "Wing", "Behind", "Abyss", "Goad", "Girlie", "Hadnt", "Tusker", "Cuff", "Nosey", "Mica", "Batten", "Column", "Lithe", "Tumour", "Hydra", "Sprung", "Foal", "Lyra", "Secede", "Sunny", "Hogger", "Teepee", "Bleach", "Sulk", "Inched", "Depot", "Mulled", "Only", "Flea", "Foam", "Tiptoe", "Jarred", "Rotted", "Edit", "Jilt", "Snowed", "Lure", "Sake", "Cram", "Riling", "Dire", "Wasp", "Duvet", "Funnel", "Alpaca", "Stable", "Yearly", "Pampas", "Jimmy", "Pluck", "Rank", "Blat", "Scent", "Gamble", "Hatred", "Shadow", "Texan", "Jazz", "Prizer", "Zipped", "Ramify", "Myrrh", "Jam", "Freon", "Fusing", "Goo", "Guitar", "Agent", "Leaver", "Sneaky", "Van", "Lea", "Sexton", "Busman", "Gloom", "Bewail", "Europe", "Reach", "Rent", "Diva", "Aint", "Health", "Gaga", "Footed", "Runway", "Virago", "Lowish", "Ignore", "Snap", "Tusked", "Abbe", "Carp", "School", "Spa", "Warder", "Rouse", "Adhere", "Junkie", "Minus", "Munch", "Pitch", "Leaned", "Miscue", "Pager", "Bobcat", "Hoar", "Cam", "Buoy", "Oilmen", "Rebus", "Gaily", "Dotty", "Brim", "Khan", "Ditto", "Obeyed", "Bridal", "Haunt", "Soffit", "Oxcart", "Rung", "Yak", "Cubing", "Spot", "Costed", "Laden", "Joyed", "Slowed", "Flurry", "Gentry", "Joist", "Perry", "Carrel", "Gave", "Pounce", "Bosun", "Pillow", "Bursar", "Jewel", "Taste", "Booboo", "Puppy", "Trait", "Missal", "Limb", "Rug", "Drowse", "Unsold", "Betray", "Lasing", "Salty", "Gallon", "Baron", "Plaice", "Blight", "Mould", "Creep", "Legacy", "Oiling", "Retch", "Jilted", "Sofa", "Domino", "Filmed", "Cagy", "Easel", "Curfew", "Vinyl", "Ashy", "Fatter", "Clique", "Should", "Enrage", "Prying", "Farmed", "Ripest", "Leeway", "Gust", "Slap", "Gamy", "Inca", "Phoney", "Inward", "Timber", "Snail", "Labium", "Spain", "Batmen", "Monroe", "Glenn", "Assign", "Stormy", "Judge", "Dell", "Disarm", "Fail", "Andes", "Maze", "Shifty", "Scout", "Rather", "Kitted", "Wine", "Dual", "Beam", "Vixen", "Biceps", "Dowdy", "Latent", "Gambit", "Height", "Fluffy", "Ogling", "Crumb", "Humid", "Elnino", "Ovoid", "Curlew", "Clasp", "Attire", "Terse", "Paste", "Dead", "Right", "Lowest", "Stripe", "Oft", "Hazy", "Haha", "Babe", "Miami", "Wended", "Weasel", "Relish", "Soared", "Id", "Fewer", "Hangar", "Buxom", "Proof", "Trout", "Exotic", "Lank", "Of", "Blurb", "Mayhap", "Tepid", "Snick", "Hatful", "Famous", "Ogrish", "Rashly", "Hexed", "Cedar", "Marred", "Meshed", "Herald", "Rake", "Loin", "Joseph", "Grassy", "Prance", "Saxony", "Testy", "Druid", "Route", "Adult", "Juno", "Dyer", "League", "Chic", "Pertly", "Mentor", "Tarred", "Grocer", "Fascia", "Comber", "Pore", "Gobble", "Globe", "Canter", "Kongo", "Bride", "Put", "Bravo", "Fodder", "Salve", "Morose", "Incest", "Duff", "Touch", "Decked", "Gusto", "Lip", "Pulsar", "Brood", "Lichen", "Larvae", "Radar", "Gammon", "Choose", "Lino", "Meaty", "Lacing", "Trough", "Gang", "Poodle", "Surfer", "Surf", "Prone", "Gawpin", "Quorum", "Ken", "Evade", "Wherry", "Fended", "Whaler", "Burned", "Lofted", "Rival", "Heave", "Shiner", "Creole", "Coral", "Loot", "Fuzzy", "Shying", "Holdup", "Litter", "Henna", "Flossy", "Pining", "Cowboy", "Speck", "Those", "Just", "Keg", "Skull", "Harp", "Whir", "Asian", "Unsent", "Coarse", "Hinny", "Soak", "Kitty", "Broker", "Mono", "Reason", "Fiche", "Thank", "Ripped", "Piety", "Spasm", "Legend", "Pegged", "Weirdo", "Hiatus", "Usury", "Thyme", "Dough", "Hare", "Plaint", "Away", "Rant", "Maim", "Brisk", "Drug", "Folder", "Revoke", "Avid", "Hawker", "Binder", "Drier", "Basked", "Rasing", "Lard", "Gandhi", "Tipple", "Hale", "Strait", "Endued", "Sodium", "Crass", "Tacked", "Bossy", "Truth", "Corner", "Ananas", "Eyelet", "Planet", "Fondly", "Lesson", "Spurge", "Girl", "Steady", "Debone", "Crusty", "Wander", "Indent", "Surtax", "Sack", "Paten", "Perky", "Astral", "Gamete", "Gap", "Peru", "Field", "Easter", "Fixed", "Ruder", "Thebes", "Pre", "Audio", "Plasm", "Smart", "Tunic", "Icicle", "Cloth", "Fryer", "Mythic", "Idling", "Jawing", "Pedlar", "Gladly", "Nosily", "Stanza", "Magma", "Wrong", "Smelt", "Oboe", "Quench", "Fiasco", "Rye", "Okayed", "Hoax", "Yelled", "Chip", "Lemon", "Frill", "Amide", "Rubies", "Lotus", "Abomb", "Fort", "Conned", "Reggae", "Morgue", "Cony", "Very", "Agouti", "Sexier", "Mulch", "Duly", "Flax", "Pecker", "India", "Ennui", "Meshes", "Keep", "Racily", "Morass", "Font", "Thymus", "Bosses", "Induna", "Rating", "Pap", "Inuit", "Orient", "Cosset", "Qatar", "Met", "Ran", "Appro", "Bake", "Dude", "Lymph", "Lancet", "Silica", "Lexeme", "Radio", "Index", "Chrome", "Effect", "Wheeze", "Snob", "Hidden", "Lieder", "Lipid", "Moses", "Wader", "Layer", "Dolly", "Alpine", "Navvy", "Barley", "Befog", "Pariah", "Gluten", "Oxtail", "Flee", "Razor", "Uvular", "Fairer", "Fez", "Dictum", "Talon", "Pipit", "Marvel", "Cot", "Derate", "Hunger", "Eosin", "Unto", "Czech", "Tyrant", "Diadem", "Shag", "Belch", "Benign", "Suffer", "Egged", "Foxing", "Vomit", "Maw", "Delate", "Cutely", "Wreak", "Eaten", "Draw", "Retail", "Me", "Sadist", "Danish", "Neuron", "Rueful", "Phlegm", "Layman", "Liven", "Enmity", "Dimly", "Luke", "Salon", "Bombay", "Wretch", "Corned", "Skulk", "Saga", "Jaw", "Foetal", "Moot", "Marked", "Flaxen", "Tract", "Handed", "Looney", "Civil", "Gaping", "Casual", "Exwife", "Redraw", "Pun", "Mutt", "Elbe", "Ruanda", "Arrest", "Tugela", "Hybrid", "Stud", "Scanty", "Pacer", "Demist", "Quaff", "Suburb", "Bonded", "Array", "Gift", "Waste", "Visage", "Concur", "Whale", "Granny", "Chose", "Snag", "Untold", "Cozier", "Pure", "Cherub", "Lazier", "Gaffe", "Dearie", "Came", "Some", "Butler", "Fire", "Gill", "Eel", "Trace", "Breeze", "Can", "Goethe", "Cede", "Bowman", "Craggy", "Pious", "Impish", "Injury", "Biopsy", "Happen", "Sear", "Lucre", "Silver", "Lintel", "During", "Decent", "Terser", "Laos", "Sexy", "Skier", "Cough", "Chore", "Belike", "Ankle", "Hazily", "Frisk", "Rubber", "Sent", "Feller", "Punk", "Nailed", "Dig", "Behest", "Prayer", "Dolman", "Spiky", "Lather", "Urgent", "Harped", "Lobbed", "Waiver", "Carbon", "Vault", "Torch", "Halo", "Haulms", "Yawned", "Horny", "Onset", "Coiner", "Meted", "Hex", "Havana", "Phase", "Jungle", "Turn", "Prowl", "Booed", "Razing", "Sinter", "Rends", "Atomic", "Mum", "Rebind", "Petty", "Gala", "Sensor", "Ashbin", "Atrial", "Bounty", "Tubing", "Wax", "Plumy", "Koala", "Tier", "Bench", "Ask", "Gaming", "Acidly", "Pack", "Meeker", "Tasty", "Wordy", "Capped", "Kebab", "Anna", "Medusa", "Hymnal", "Roamed", "Claw", "Marble", "New", "Ding", "Legman", "Resign", "Donga", "Rehash", "Hussy", "Arctic", "Honshu", "Exeunt", "Heady", "Dopey", "Neatly", "Heart", "Muddy", "Condor", "Canted", "Tilt", "Eared", "Morbid", "Brash", "Velar", "Shawl", "Peered", "Muting", "Baited", "Crisis", "Eta", "Budgie", "Outran", "Poor", "Taller", "Parsec", "Fallow", "Sculpt", "Manly", "Polled", "Dank", "Maroon", "Parka", "Moping", "Live", "Entice", "Chief", "Jetted", "Into", "Prize", "Camp", "Titter", "Orca", "Beagle", "Easily", "Axial", "Sprint", "Gamma", "Gather", "Froze", "Manse", "Slay", "Engage", "Swamp", "Fruit", "Ninety", "Graham", "Lamest", "Cope", "Modish", "Exes", "Avens", "Big", "Pompey", "Regime", "Muons", "Shaky", "Rhea", "Paler", "Boldly", "Air", "Wool", "Animal", "Chart", "Fender", "Hotrod", "Paddle", "Magnum", "Numb", "Idea", "Eerily", "Reheat", "Shanty", "Debit", "Sat", "Locust", "Snore", "Minute", "Rashes", "Pew", "Meter", "Nutty", "Fizzy", "Alp", "Edify", "Grease", "Bergs", "Age", "Cake", "Curate", "Fief", "Ahem", "Repay", "Jerky", "Pry", "Decide", "Tackle", "Garbed", "Hashes", "Abloom", "Travel", "Preamp", "Shorn", "Use", "Welter", "Swept", "Stew", "Dubbed", "Oscar", "Hurl", "Headon", "War", "Shook", "Hefty", "Bunny", "Peck", "Chock", "Tidbit", "Thirty", "Media", "Tiring", "Daffy", "Racer", "Queue", "Pygmy", "Cheek", "Permit", "Went", "Annal", "Dulcet", "Sleazy", "Am", "Parted", "Rod", "Glut", "Ermine", "Eclair", "Bagdad", "Piling", "Podia", "Police", "Bronze", "Fiance", "Ursine", "Mute", "Raging", "Bicarb", "Faint", "Orally", "Loner", "Furled", "Bracer", "Midas", "Oops", "Glitzy", "Delphi", "Bailed", "Melt", "Airmen", "Axing", "Upside", "Floppy", "Tsetse", "Dioxin", "Alert", "Prig", "Begged", "Arbor", "Fault", "Mast", "Fencer", "Rivet", "Derive", "Eatery", "World", "Chain", "Midday", "Sexual", "Little", "Mutant", "Septic", "Seacow", "Myself", "Laid", "Lumped", "Sitar", "Redsea", "Ascend", "Wooded", "Madden", "Chuck", "Seabed", "Calory", "Verity", "Wedded", "Ion", "Asses", "Mufti", "Deejay", "Encode", "Billy", "Jaunt", "Wigeon", "Legate", "Pricy", "Hunk", "Beauty", "Abroad", "Ledge", "Inbred", "Whim", "Fool", "Dash", "Armies", "Ram", "Wicked", "Oxalic", "Decamp", "Admire", "Gummed", "Chef", "Robe", "Shyer", "Martin", "Bully", "Busies", "Bitmap", "Dried", "Palled", "Finish", "Aside", "Fogged", "Plucky", "Mosaic", "Parch", "Switch", "Nearby", "Lowkey", "Defray", "Dont", "Kalif", "Minor", "Polyp", "Fest", "Revive", "Murder", "Lambda", "Mourn", "Beefy", "Passer", "Ensue", "Clay", "Lipase", "Wanly", "Java", "Grin", "Coax", "Kick", "Lawyer", "Encage", "Moor", "Thorn", "Nude", "Cadge", "Uglier", "Furies", "Ease", "Lend", "Facing", "Uncoil", "Line", "Method", "Lasses", "Ufo", "Cretan", "Sludgy", "Eat", "Flog", "Peddle", "Malted", "Bonnie", "Chilly", "Ulster", "Export", "Hurt", "Augite", "Wore", "Gorgon", "Seduce", "Stench", "Hovel", "Frumpy", "Atoll", "Vellum", "Pastry", "Clerk", "Idol", "Gluon", "Stub", "Junk", "Devoid", "Calif", "Device", "Maoist", "Burden", "Dye", "Whinny", "Cancan", "Format", "Glower", "Wry", "Brick", "Goody", "Slogan", "Kenyan", "Ride", "Bulb", "Looker", "Equate", "Sowed", "Around", "Flatus", "Dogood", "Slider", "Adhoc", "Offal", "Tonne", "Burn", "Foulup", "Mar", "Muck", "Spook", "Zeta", "Ailing", "Uhuh", "Jacob", "Feint", "Satrap", "Won", "Polite", "Schist", "Danger", "Reject", "Cuss", "Puffy", "Racoon", "Shrank", "Lethal", "Photon", "Thaw", "Strode", "Appal", "Father", "Gobi", "Damson", "Mowed", "Rum", "Fluxes", "Refile", "Cannot", "Spool", "Seeker", "Pouter", "Winnow", "Filled", "Busied", "Plate", "Bitch", "Loaner", "Auric", "Down", "Leap", "Bangle", "Gayest", "Edible", "Sailed", "Scathe", "Newsy", "Gag", "Rasped", "Liquid", "Molest", "Late", "Oozy", "Tuna", "Summer", "Valuer", "Deb", "Aqua", "Cirrus", "Loping", "Poult", "Kinked", "Grimy", "Kraft", "Dorado", "Mummy", "Hour", "Maumau", "Poise", "Wigwam", "Elvish", "Sheath", "Guess", "Pent", "Much", "Rate", "Dental", "Luther", "Calf", "Cravat", "Utah", "Rumba", "Breezy", "Trice", "Nipple", "Each", "Rebuke", "Dread", "Drafty", "Tube", "Payee", "Nickel", "Dither", "Aviate", "Licked", "Swag", "Formal", "Clever", "Faxed", "Malva", "Smirk", "Cutter", "Dryer", "Rewrap", "Blitz", "Madder", "Laud", "Godly", "Lector", "Decry", "Baleen", "Unfed", "Man", "Ace", "Gipsy", "Booze", "Uganda", "Doily", "Tide", "Wolf", "Sire", "That", "Boxed", "Hungry", "Mushy", "Joiner", "Pall", "Renter", "Sooty", "Tremor", "Aback", "Stager", "Eatage", "Sigma", "Comet", "Downed", "Fitly", "Peking", "Resort", "Race", "Myopia", "Themas", "Carry", "Wagon", "Quebec", "Valve", "Asking", "Frail", "Foxed", "Trial", "Erring", "Prefer", "Fescue", "Chalet", "Outset", "Peanut", "Tern", "Byway", "Saucer", "Piano", "Weight", "Ale", "Kith", "Sotho", "Loco", "Egress", "Swipe", "Tenet", "Eyeing", "Typify", "Divide", "Siting", "Geek", "Gully", "Dram", "Cashes", "Firing", "Chair", "Yoyo", "Decree", "Jack", "Scoop", "Dairy", "Shun", "Family", "Lope", "Gazing", "Roared", "Llama", "Wildly", "Taut", "Hollow", "Often", "Hate", "Stoma", "Baling", "Matron", "Mettle", "Roach", "Hourly", "Feebly", "Double", "Tares", "Patrol", "Niger", "Prang", "Golfer", "Nabbed", "Mitten", "Moral", "Cramp", "Naming", "Punted", "Parson", "Mooed", "Paul", "Reload", "Worm", "Hatted", "Wizard", "Norm", "Ribald", "Browse", "Rage", "Cretin", "Ferry", "Boggle", "Sachet", "Muscat", "Valise", "Cone", "Swathe", "Dormer", "Pork", "Ransom", "Mendel", "Grub", "Kite", "Lifter", "Dicing", "Ghoul", "Twig", "Havent", "Stray", "Ileum", "Graver", "Flumes", "Poking", "Opioid", "Heeded", "Wining", "Clump", "Botany", "Pogo", "Pallid", "Lowing", "Relit", "Unbend", "Carton", "Quirky", "Gory", "Banker", "Massif", "Tories", "Winch", "Hogged", "Hugely", "Evince", "Cocky", "Ravage", "Web", "Tamper", "Mucked", "Pick", "Belief", "Filet", "Star", "Sing", "Astute", "Isms", "Spurn", "Tipoff", "Edited", "Gerund", "Hoary", "Harmer", "Thrall", "Vapour", "Pug", "Genoa", "Shaw", "Judder", "Cruxes", "Stove", "Diesel", "Misty", "Insist", "Bowing", "Bonus", "Chalk", "Virus", "Arcade", "Bisect", "Print", "Wilder", "Brazen", "Canaan", "Decade", "Wow", "Kaftan", "Coxed", "Pierce", "Grab", "Senate", "Ranger", "Borneo", "Bond", "Tossup", "Humify", "Fusty", "Fanny", "Spoken", "Forty", "Dawn", "Limeys", "Qua", "Quiche", "Known", "Exam", "Inept", "Whorls", "Bonobo", "Pigsty", "Bless", "Herded", "Awaken", "Paw", "Enfold", "Daze", "Ugly", "Woman", "Hello", "Fear", "Mess", "Crane", "Harem", "Guider", "Short", "Refer", "Sang", "Censer", "Puff", "Zealot", "Calmer", "Relic", "Formic", "Cellar", "Leaf", "Boohoo", "Shall", "Iraqi", "Hart", "Wiper", "Makeup", "Gorier", "Knight", "Ref", "Normal", "Scowl", "Headed", "Jute", "Hem", "Sump", "Sydney", "Larva", "Usher", "Corgi", "Silage", "Levi", "Retune", "Set", "Ended", "Except", "Lusted", "Nicety", "Trendy", "Driver", "Rancid", "Baulk", "Cuckoo", "Tubby", "Gaze", "Burgle", "Sought", "Trikes", "Hoaxed", "Revue", "Cleft", "There", "Toll", "Archer", "Winded", "Graven", "Beau", "Hash", "Shaven", "Pawned", "Beyond", "Absurd", "Rasher", "Whip", "Memoir", "Liver", "Muse", "Tumble", "Frayed", "Useful", "Mirage", "Squirt", "Chopin", "Spare", "Spell", "Skew", "Lean", "Fiord", "Hire", "Amour", "Chunky", "Darwin", "Ramrod", "Whee", "Elbow", "Gdansk", "Octane", "Staple", "Gold", "Duo", "Ratio", "Void", "Rudely", "Rearm", "Baring", "Glint", "Shoo", "Erg", "Rim", "Watch", "Expert", "Slant", "Tuck", "Stark", "Veil", "Screw", "Acacia", "Owl", "Debunk", "Viable", "Volt", "Kid", "Squint", "Pile", "Depart", "Meek", "Hobbit", "People", "Ooh", "Hue", "Tenant", "Lupin", "Rainy", "Added", "Ablaze", "Link", "Gospel", "Agave", "Poet", "Lotion", "Abies", "Mite", "Pisa", "Orphan", "Hewing", "Viola", "Faulty", "Vex", "Codex", "Setts", "Obey", "Gunned", "Feud", "Duchy", "Aerobe", "Lackey", "Gird", "Edge", "Hair", "Day", "Rut", "Pub", "Gaunt", "Rift", "Bogy", "Annexe", "Avoid", "Peruse", "Aaron", "Kilter", "Grime", "Diet", "Ochre", "Apsis", "Peak", "Exhort", "Ingest", "Nephew", "Ore", "Dotted", "Petrel", "Fume", "Etna", "Mile", "Menhir", "Gauze", "Chink", "Relink", "Cranky", "Sign", "Mound", "Roan", "Pagan", "Teapot", "Rwanda", "Ureter", "Cosily", "Linkup", "Detach", "Early", "Maxim", "Whacko", "Tissue", "Pylon", "Laymen", "Vetted", "Fifth", "Umlaut", "Soused", "Evenly", "Punt", "Crises", "Backer", "Mazier", "Outbid", "Unset", "Employ", "Foamed", "Witch", "Broody", "Splash", "Hissed", "Welt", "Bulge", "Odd", "Lauded", "Hushed", "Bold", "Recast", "Pocked", "Louche", "Mac", "Bounce", "Staff", "Cater", "Thrice", "Lease", "Beef", "Aegean", "Irk", "Lava", "Barb", "Cade", "Drippy", "Packed", "Choral", "Masque", "Scaly", "Usage", "Wallow", "Sequin", "Isomer", "Greedy", "Nile", "Muffle", "Wifely", "Length", "Camped", "Blase", "Apnea", "Minion", "Prong", "Agleam", "Banana", "Hoof", "Acetal", "Dreamy", "Influx", "Redder", "Instep", "Silo", "Eczema", "Unison", "Ridge", "Shower", "Cavein", "Lapdog", "Talent", "Cast", "Ageism", "Brio", "Genets", "Kitbag", "Ahoy", "Paunch", "Stuck", "Thesis", "Troy", "Soured", "Depict", "Oozing", "Quilt", "Sander", "Burrow", "Gramme", "True", "Header", "Rakish", "Volute", "Flyer", "Arose", "Misery", "Spoke", "Parrot", "Loiter", "Laying", "Artful", "Slink", "Hoe", "Crim", "Doggy", "Gavial", "Meanly", "Bung", "Oldage", "Smiler", "Poof", "Herm", "Ringer", "Both", "Seesaw", "Opine", "Urn", "Fine", "Annoy", "Shoddy", "Storey", "Kenya", "Floss", "Remedy", "Lubber", "Loaned", "Velum", "Saute", "Fringy", "Prissy", "Inlet", "Blonde", "Maxima", "Chiral", "Poem", "Cloven", "Foe", "Askers", "Volume", "Rhymer", "Humane", "Formed", "Tip", "Beaten", "Kidnap", "Lopped", "Induce", "Woke", "Victor", "Trod", "Ouch", "Incite", "Rifle", "Aden", "Cafe", "Buffer", "Unify", "Zany", "Worser", "Fondue", "Befit", "Darn", "Trigs", "Turtle", "Clout", "Tidies", "Hereto", "Feet", "River", "Pest", "Indigo", "Sealer", "Pang", "Fonder", "Elysee", "Delay", "Look", "Bluest", "Unfirm", "Subset", "Finale", "Extol", "Eaglet", "Calico", "Bomb", "Budge", "Output", "Docked", "Chekov", "Female", "Shyest", "Surd", "Fluor", "Couple", "Culled", "Leash", "Pussy", "Fishes", "Allay", "Easing", "Trim", "Vodka", "Grim", "Profit", "Lusher", "Slut", "Refute", "Pup", "Pajama", "Scenic", "Hatter", "Dew", "Murray", "Always", "Teller", "Solace", "Nigh", "Suite", "Blouse", "Missy", "Russia", "Mat", "Nay", "Slur", "Sagged", "Sis", "Propel", "Wayout", "Moth", "Soothe", "Em", "Colour", "Luring", "Braw", "Natal", "Fond", "Limbo", "Geared", "Wren", "Menace", "Tee", "Genre", "Recto", "Noun", "Sane", "Gothic", "Prow", "Endear", "Detect", "Wisely", "Apply", "Lurker", "Cress", "Dressy", "Stumpy", "Kiln", "Braze", "Stucco", "Aver", "Tacit", "Tether", "Hosing", "Burp", "Cobweb", "Dative", "Beacon", "Empty", "Bridge", "Testes", "Fungus", "Huff", "Worry", "Lynxes", "Laxity", "Debase", "Ire", "Basify", "Snipe", "Gosh", "Dearly", "Pony", "Bustle", "Blink", "Doll", "Remind", "Kettle", "Toss", "Basket", "Haiti", "Jangle", "Bushy", "Sic", "Nebula", "Piglet", "Lysine", "Dachau", "Any", "Burst", "Screwy", "Cosmos", "Afoot", "Paired", "Lathe", "Using", "Acquit", "Deputy", "Gloss", "Mopped", "Boson", "Flout", "Ploy", "Ship", "Oner", "Pa", "Pixel", "Nipped", "Mom", "Bunk", "Valuta", "Ordeal", "Wright", "Argot", "Bus", "Float", "Unpack", "Rapier", "Filly", "Hew", "Champ", "Puny", "Romped", "Sluice", "Vat", "Roe", "Deluge", "Graph", "Retina", "Hinge", "Jigsaw", "Slag", "Pectin", "Bushel", "Roll", "Covet", "Boor", "Necked", "Deify", "So", "Nib", "Royal", "Ergo", "Spun", "Noise", "Peachy", "Pleura", "Gemmed", "Runt", "Boa", "Outdo", "Chosen", "Puking", "Mozart", "Drake", "Flub", "Thus", "Now", "Sushi", "Afresh", "Tarzan", "Repack", "Est", "Dredge", "Anti", "View", "Outlay", "Nutmeg", "Hombre", "Joey", "Zebu", "Rag", "Unmade", "Canada", "Cosier", "Lick", "Shire", "Clang", "Darted", "Swivel", "Detail", "Crate", "Festal", "Sighed", "Subdue", "Adapt", "Orbit", "Sale", "Prefab", "Urchin", "Rill", "Plop", "Apron", "Coward", "Viewed", "Lazuli", "Frozen", "Utmost", "Fierce", "Tech", "Clash", "Wash", "Furore", "Ho", "Inkier", "Cocoon", "Mob", "Dowse", "Sherds", "Bay", "Hymn", "Yon", "Cinder", "Mammal", "Armpit", "Teem", "Voting", "Adieu", "Fidget", "User", "Aline", "Ribbon", "Cigar", "Mashed", "Oldie", "Cooler", "Votive", "Poser", "Parley", "Dull", "Port", "Trader", "Bigger", "Losing", "Tartar", "Newton", "Nasal", "Mutual", "Weigh", "Kuwait", "Fag", "Remix", "Tamer", "Papery", "Tulip", "Downy", "Scant", "Munich", "Lumber", "Amaze", "Dowser", "Clog", "Quest", "Rector", "Sidle", "Buying", "Beckon", "Borne", "Porno", "Tranny", "Basalt", "Blaze", "Botch", "Hanger", "Seer", "Chives", "Envies", "Goer", "Pinup", "Foil", "Chase", "Wast", "Sordid", "Bolder", "Small", "Echoed", "Adroit", "Khalif", "Truer", "Leaner", "Pal", "Croon", "Singe", "Parity", "Marx", "Primal", "Bingo", "Scale", "Disc", "Gay", "Canny", "Syntax", "Woofer", "Deco", "Hakea", "Sock", "Teak", "Horde", "Shiny", "Gaff", "Centre", "Diaper", "Lovely", "Styx", "Balder", "Starer", "Atrium", "Tapas", "Lenin", "Pinker", "Guilt", "Slob", "Hadron", "Cyst", "Brewed", "Whiff", "Heap", "Ionic", "Jamb", "Murk", "Cobalt", "Tipped", "Thai", "Minima", "Regret", "Crown", "Junior", "Welly", "Biking", "Other", "Ahead", "Nervy", "Plot", "Waffle", "Pretor", "Cub", "Pumice", "Goaded", "Sparse", "Vogue", "Jetsam", "Chad", "Angler", "Comely", "Fairly", "Facile", "Raring", "Opener", "Hooves", "Weave", "Oxygen", "Lot", "Noel", "Curled", "Dante", "Cower", "Minx", "Molts", "Anomic", "Held", "Indite", "Irate", "Catchy", "Jokier", "Deafer", "Exert", "Emir", "Ferric", "Hopper", "Bloat", "Aglow", "Hard", "Dally", "Third", "Skinny", "Ramble", "Kloof", "Uncle", "Venom", "Mix", "Haloed", "Bawled", "Wooly", "Wove", "Italic", "Oaken", "Sauna", "Bionic", "Bijou", "Jap", "Bodice", "Rally", "Mason", "Rich", "Purred", "Mesh", "Pus", "Poring", "Weir", "Tan", "Fob", "Tinge", "Drawn", "Degree", "Faroff", "Toy", "Unary", "Ironic", "Ebb", "Dopier", "Winner", "Flay", "Peri", "Latex", "Wooed", "Socked", "Deist", "Stern", "Expire", "Lodge", "Fellow", "Gargle", "Hooray", "Purims", "Enjoy", "Nodded", "Turbo", "Dement", "Soaped", "Judas", "Aerify", "Unkept", "Master", "Meson", "Fatal", "Jotter", "Isnt", "Hiving", "Match", "Neaten", "Creme", "Vary", "Monk", "Serif", "Sicken", "Mall", "Gagged", "Amoral", "Wetly", "Twine", "Dummy", "Raver", "Batman", "Taunt", "Zoo", "Unwise", "Polity", "Flinty", "Coxes", "Leeds", "Snort", "Rarer", "Sissy", "Vanish", "Wane", "Minim", "Washes", "Cagey", "Hauled", "Fiery", "Blunt", "Rumble", "Xrayed", "Milieu", "Yoga", "Barely", "Cork", "Fought", "Vale", "Nougat", "Zees", "Argue", "Unfold", "Erase", "Errant", "Alkyl", "Phooey", "Giddy", "Lyon", "Wodan", "Dearth", "Sheer", "Sating", "Peel", "Hereby", "Knot", "Flask", "Quill", "Sure", "Embody", "Mincer", "Aswan", "Effigy", "Cabal", "Minty", "Braid", "Ail", "Sledge", "Homing", "Song", "Scarce", "Stoep", "Tilted", "Mural", "Dug", "Pet", "Zone", "Deeply", "Belt", "Smudgy", "Guest", "Hake", "Bout", "Rangy", "Govern", "Lama", "Putter", "Awe", "Stator", "Tuareg", "Lustre", "Anode", "Upkeep", "Cotton", "Rude", "Silt", "Palm", "Term", "Oinked", "Warped", "Fact", "Biz", "Knives", "Quern", "Flick", "Whelk", "Wether", "Shaver", "Basque", "Remain", "Sung", "Dilate", "Supply", "Stave", "Bereft", "Waning", "Alack", "Reefed", "Gangly", "Stomp", "Plum", "Incant", "Quote", "Gaijin", "Honour", "Healed", "Paling", "Slalom", "Shaft", "Bong", "Wetter", "Throb", "Snug", "Cola", "Tenser", "Mutely", "Treaty", "Unseat", "Issuer", "Hamper", "Evened", "Fret", "Bonbon", "Exceed", "Tiara", "Ewe", "Sup", "Wonder", "Yolk", "Loomed", "Ripoff", "Bait", "Teamed", "Glob", "Etcher", "Slyly", "Jitter", "Quart", "Dump", "Denial", "Indoor", "Zephyr", "Slot", "Crag", "Tour", "Pi", "Steer", "Egg", "Raided", "Clink", "Bowed", "Rain", "Became", "Moaned", "Usurp", "Antrum", "Hearty", "Ageold", "Googly", "Robbed", "Midi", "Target", "Talcum", "Gasket", "Leaky", "Hotel", "Thor", "Abaft", "Heater", "Fluid", "Lying", "Lasted", "Jet", "Native", "Realm", "Seared", "Belly", "Donor", "Murky", "Fame", "Capita", "Glen", "Pen", "Groggy", "Polish", "Light", "Fibre", "Yacht", "Flesh", "Drill", "Saber", "Gut", "Boon", "Worn", "Garage", "Venial", "Sudan", "Pier", "Aloha", "Bid", "Rental", "Fowl", "Puma", "Feign", "Sortie", "Duiker", "Dollar", "Banded", "Crocus", "Timer", "Bribe", "Crete", "Ton", "Madam", "Feeble", "Captor", "Myelin", "Iota", "Beset", "Unduly", "Sip", "Abseil", "Tues", "Papua", "Tax", "Betel", "Later", "Sag", "Jugged", "Typist", "Cuboid", "Maybe", "Jingle", "Saddle" };
        private string connectionString;

        public PrecogPipeline(PrecogPipelineOptions precogPipelineOptions, ILoggerFactory loggerFactory)
        {
            opt = precogPipelineOptions;
            logger = loggerFactory.CreateLogger("Precog");
        }

        public override void Register(ICollection<Filter> filters)
        {
            logger.LogTrace($"{GetType().Name} -> Register");
            logger.LogInformation(opt.ToString());

            // Settings
            ConnectionOptionsBuilder optionsBuilder = new ConnectionOptionsBuilder();
            ConnectionOptions connectionOptions = optionsBuilder
                .Url(opt.Twinzo.TwinzoBaseUrl)
                .Client("Infotech")
                .ClientGuid(opt.Twinzo.ClientGuid.ToString())
                .BranchGuid(opt.Twinzo.BranchGuid.ToString())
                .ApiKey(opt.Twinzo.ApiKey)
                .Timeout(opt.Twinzo.Timeout)
                .Version(ConnectionOptions.VERSION_3)
                .Build();
            var connectorV3 = (DevkitConnectorV3)DevkitFactory.CreateDevkitConnector(connectionOptions);
            var memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromSeconds(10)
            });
            var cacheConnector = new DevkitCacheConnector(connectorV3, memoryCache);
            cacheConnector.ExpirationInSeconds = opt.MemoryCache.ExpirationInSeconds;

            // Channels
            var hubChannel = Channel.CreateUnbounded<PartitionEvent>();
            var precogBeaconChannel = Channel.CreateUnbounded<PrecogBeacon>();
            var precogDataChannel = Channel.CreateUnbounded<PrecogData>();
            var aggregatorChannel = new ConcurrentDictionary<long, AggregateData>();
            var locationChannel = Channel.CreateUnbounded<DeviceLocationContract>();
            connectionString = opt.MsSqlConnectionString;

            // Pipeline
            var hubFilter = new EventHubReaderFilter(hubChannel.Writer, opt.EventHubConnectionString, opt.ConsumerGroup);
            var timerFilter = new TimerFilter(hubFilter, 100, 1, 1);

            //var output = new TempOutputFilter(hubChannel.Reader, opt.MsSqlConnectionString);

            var transform = new TransformChannelFilter<PartitionEvent, PrecogBeacon>(hubChannel.Reader, precogBeaconChannel.Writer, Transform2);
            var transform2 = new Transformator2Filter(precogBeaconChannel.Reader, precogDataChannel.Writer);
            var aggregator = new AggregatorFilter(precogDataChannel.Reader, aggregatorChannel, opt.AggregationIntervalMillis);

            var trillateration = new TrillaterationFilter(aggregatorChannel, locationChannel.Writer, cacheConnector, opt.AggregationIntervalMillis, new TrilaterationStrategy());
            var trilaterationTimerFilter = new TimerFilter(trillateration, opt.AggregationIntervalMillis, 1, 1);

            var senderFilter = new RtlsSenderFilter(locationChannel.Reader, connectorV3, opt.RtlsSender.SendIntervalMillis, opt.RtlsSender.MaxSize);

            filters.Add(timerFilter);
            //filters.Add(output);
            filters.Add(transform);
            filters.Add(transform2);
            filters.Add(aggregator);
            filters.Add(trilaterationTimerFilter);
            filters.Add(senderFilter);

            foreach (var filter in filters)
            {
                filter.Logger = logger;
            }
        }

        protected PrecogBeacon Transform2(PartitionEvent partitionEvent)
        {
            try
            {
                var beacon = JsonSerializer.Deserialize<PrecogBeacon>(partitionEvent.Data.EventBody);

                var deviceId = partitionEvent.Data.SystemProperties["iothub-connection-device-id"].ToString();
                var beaconSerialName = deviceId.ToUpper();
                if (beaconSerialName.Length >= 12)
                {
                    beaconSerialName = beaconSerialName.Substring(0, 12);
                }

                if (beaconSerialName.Length > 0)
                {
                    beacon.Name = beaconSerialName;
                }

                var probeDateTime = partitionEvent.Data.SystemProperties["iothub-enqueuedtime"].ToString();
                DateTime pDate = DateTime.Parse(probeDateTime);
                probeDateTime = pDate.ToString("yyyy-MM-dd HH:mm:ss");

                var sqlCommand = GetSQLInsert(deviceId, beaconSerialName, probeDateTime, partitionEvent.Data.EventBody.ToString());
                InsertData(sqlCommand);
                Console.WriteLine(partitionEvent.Partition.PartitionId + " " + partitionEvent.Data.EnqueuedTime + " " + beacon.Name);
                return beacon;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "");
            }
            return null;
        }

        private string GetSQLInsert(string deviceId, string beaconSerialName, string probeDateTime, string encodedArray)
        {
            var beaconTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // instead of creating the variable devicePacket here, create outside this function and pass in
            Root devicePacket = JsonSerializer.Deserialize<Root>(encodedArray);

            Console.WriteLine(devicePacket.m);

            // If the beaconSerialName has been generated outside the function, then set the new name
            if (beaconSerialName.Length > 0)
            {
                devicePacket.m = beaconSerialName;
            }
            devicePacket.deviceid = deviceId;

            var comma = "";
            string insert_data = "";

            foreach (var item in devicePacket.d)
            {
                // Convert hex fingerprint to ASCII string
                item.fp = HexadecimalCSVToASCIICSV(item.h);

                // Convert mac address to four words
                item.fw = macAddressToFourWords(item.m);

                // Convert access point mac address to four words
                item.apfw = macAddressToFourWords(item.cm);

                insert_data = insert_data + comma + "(";

                if (item.apfw.Length > 0)
                {
                    item.cm.PadRight(12, '0');
                    insert_data = insert_data + "'" + item.apfw + "'";                              // accesspointfourwords
                    insert_data = insert_data + "," + "'" + item.cm.Substring(0, 6) + "'";          // accesspoint_oui
                    insert_data = insert_data + "," + "'" + item.cm.Substring(6, 6) + "'";          // accesspoint_addr
                }
                else
                {
                    insert_data = insert_data + "null";                                             // accesspointfourwords
                    insert_data = insert_data + "," + "null";                                       // accesspoint_oui
                    insert_data = insert_data + "," + "null";                                       // accesspoint_addr
                };

                insert_data = insert_data + "," + "'" + devicePacket.m + "'";                       // beaconid (deviceserialname)
                insert_data = insert_data + "," + "'" + beaconTime + "'";                           // beacontime
                insert_data = insert_data + "," + item.d;                                           // dwelltime

                insert_data = insert_data + "," + "'" + item.et + "'";                                          // endtimeutc
                insert_data = insert_data + "," + item.et.Replace("-", "").Replace(" ", "").Replace(":", "");    // endtimeutcdatetimeminuteskey

                if (item.f == 1)    // this is an access point, so there will not be a fingerprint
                {
                    insert_data = insert_data + "," + "null";                                       // fingerprint
                    insert_data = insert_data + "," + "null";                                       // fingerprint_a
                    insert_data = insert_data + "," + "0";                                          // fingerprint count
                }
                else
                {
                    if (item.h.Length > 0) // we have a fingerprint
                    {
                        insert_data = insert_data + "," + "'" + item.h + "'";                       // fingerprint
                        insert_data = insert_data + "," + "'" + item.fp.Replace("'", "''") + "'";   // fingerprint_a
                        insert_data = insert_data + "," + "'" + item.h.Split(",").Length + "'";     // fingerprint count
                    }
                    else
                    {
                        insert_data = insert_data + "," + "null";                                   // fingerprint
                        insert_data = insert_data + "," + "null";                                   // fingerprint_a
                        insert_data = insert_data + "," + "0";                                      // fingerprint count
                    }
                }
                insert_data = insert_data + "," + "4";                                              // firmwareversion

                insert_data = insert_data + "," + "'" + item.fw + "'";                              // fourwords
                insert_data = insert_data + "," + item.f;                                           // frametype
                insert_data = insert_data + "," + "0";                                              // messagetype

                item.m.PadRight(12, '0');
                insert_data = insert_data + "," + "'" + item.m.Substring(0, 6) + "'";               // oui
                insert_data = insert_data + "," + "'" + item.m.Substring(6, 6) + "'";               // addr

                insert_data = insert_data + "," + "'" + probeDateTime + "'";                        // probedatetimeutc
                insert_data = insert_data + "," + probeDateTime.Replace("-", "").Replace(" ", "").Replace(":", "");           // probedatetimeutcdatetimeminuteskey

                insert_data = insert_data + "," + item.r;                                           // rssi

                if (item.f != 1)    // this is not an access point, so there will not be a value for the ssid
                {
                    insert_data = insert_data + "," + "null";                                       // ssid
                    insert_data = insert_data + "," + "null";                                       // ssid_a
                }
                else
                {
                    insert_data = insert_data + "," + "'" + item.h + "'";                           // ssid
                    insert_data = insert_data + "," + "'" + item.fp.Replace("'", "''") + "'";       // ssid_a
                }

                insert_data = insert_data + "," + "'" + item.st + "'";                                           // starttimeutc
                insert_data = insert_data + "," + item.st.Replace("-", "").Replace(" ", "").Replace(":", "");    // starttimeutcdatetimeminuteskey

                insert_data = insert_data + "," + item.et.Substring(0, 8).Replace("-", "").Replace(" ", "").Replace(":", "");    // enddaykey

                insert_data = insert_data + "," + "'gen2'";                                         // streamsname

                insert_data = insert_data + "," + "'" + item.c.Replace("'", "") + "'";                               // apcountryid
                insert_data = insert_data + "," + item.ch;                                          // channel

                insert_data = insert_data + "," + "'" + beaconSerialName + "'";                     // beaconserialname
                insert_data = insert_data + "," + "'" + deviceId + "'";                             // beacondeviceid

                insert_data = insert_data + ")";
                comma = ",";

                // generate data for JSON structure

            }

            var a = CreateInsert(insert_data);
            return a;
        }

        static string CreateInsert(string data)
        {
            return
            @$"With cte
            AS (
                SELECT * FROM
                    ( VALUES
                        {data}
                    ) AS T
                (
                    accesspointfourwords, accesspoint_oui, accesspoint_addr, beaconid, beacontime, dwelltime, endtimeutc, endtimeutcdatetimeminuteskey,
                    fingerprint, fingerprint_a, fingerprint_count, firmwareversion, fourwords, frametype, messagetype, oui, addr, probedatetimeutc, probedatetimeutcdatetimeminuteskey,
                    rssi, ssid, ssid_a, starttimeutc, starttimeutcdatetimeminuteskey, enddaykey, streamname, apcountryid, channel, beaconserialname, beacondeviceid
                )
            )
            Insert INTO RawData
            SELECT * FROM cte;

            With cte
            AS (
                SELECT * FROM
                    ( VALUES
                        {data}
                    ) AS T
                (
                    accesspointfourwords, accesspoint_oui, accesspoint_addr, beaconid, beacontime, dwelltime, endtimeutc, endtimeutcdatetimeminuteskey,
                    fingerprint, fingerprint_a, fingerprint_count, firmwareversion, fourwords, frametype, messagetype, oui, addr, probedatetimeutc, probedatetimeutcdatetimeminuteskey,
                    rssi, ssid, ssid_a, starttimeutc, starttimeutcdatetimeminuteskey, enddaykey, streamname, apcountryid, channel, beaconserialname, beacondeviceid
                )
            )
            INSERT INTO MacAddress(fourwords, address, oui, creationdate)
            SELECT DISTINCT fourwords, addr as address, oui, probedatetimeutcdatetimeminuteskey as creationdate
            FROM cte";
        }

        private int InsertData(string sqlInsert)
        {
            int retRows = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                Console.Out.WriteLine("Opening connection");
                conn.Open();

                using (SqlCommand command = new SqlCommand(sqlInsert, conn))
                {
                    try
                    {
                        retRows = command.ExecuteNonQuery();
                        //log.LogInformation($"\nDB rows affected: {aff}\n");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "");
                        //log.LogInformation($"\nError encountered during Raw Data INSERT operation\n");
                        retRows = -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return retRows;
        }

        public static string HexadecimalCSVToASCIICSV(string hex)
        {
            string retAsciiString = "";
            string comma = "";

            if (hex.Length > 0)
            {
                string[] words = hex.Split(',');
                foreach (var word in words)
                {
                    try
                    {
                        retAsciiString = retAsciiString + comma + HexadecimalToASCII(word);
                        comma = ",";
                    }
                    catch
                    {
                        // do nothing
                    }
                }
            }
            return retAsciiString;
        }

        public static string HexadecimalToASCII(string hex)
        {
            string ascii = string.Empty;

            // if we have an odd length, add a trailing 0
            if (hex.Length % 2 == 1) // odd length
            {
                hex = hex + '0';
            }

            for (int i = 0; i < hex.Length; i += 2)
            {
                ascii += (char)HexadecimalToDecimal(hex.Substring(i, 2));
            }

            return ascii;
        }

        private static int HexadecimalToDecimal(string hex)
        {
            hex = hex.ToUpper();

            int hexLength = hex.Length;
            double dec = 0;

            for (int i = 0; i < hexLength; ++i)
            {
                byte b = (byte)hex[i];

                if (b >= 48 && b <= 57)
                {
                    b -= 48;
                }
                else if (b >= 65 && b <= 70)
                {
                    b -= 55;
                }

                dec += b * System.Math.Pow(16, ((hexLength - i) - 1));
            }

            return (int)dec;
        }
        public static string macAddressToFourWords(string hex)
        {
            int[] intArr = new int[4];

            string ouiSeed = "Planes fly south In the Winter 2";
            string addressSeed = "PigS sw1m in hot mud baths bac0n";
            string fourWords = "";

            if (hex.Length > 0)
            {
                fourWords = "Unknown.Unknown.Unknown.Unknown";
                try
                {
                    string x = hex.Replace(":", "").PadRight(15, '0');

                    byte[] y = Encoding.ASCII.GetBytes(x);

                    string ouihex = encHex(y.Take(6).ToArray(), Encoding.ASCII.GetBytes(ouiSeed));
                    string addrhex = encHex(y.Skip(6).Take(6).ToArray(), Encoding.ASCII.GetBytes(addressSeed));

                    //string words = ouihex.Replace("-", "").Substring(0, 6) + "||" + addrhex.Replace("-", "").Substring(0, 6);

                    intArr[0] = int.Parse(ouihex.Replace("-", "").Substring(0, 3), System.Globalization.NumberStyles.HexNumber);
                    intArr[1] = int.Parse(ouihex.Replace("-", "").Substring(3, 3), System.Globalization.NumberStyles.HexNumber);
                    intArr[2] = int.Parse(addrhex.Replace("-", "").Substring(0, 3), System.Globalization.NumberStyles.HexNumber);
                    intArr[3] = int.Parse(addrhex.Replace("-", "").Substring(3, 3), System.Globalization.NumberStyles.HexNumber);

                    fourWords = wordArray[intArr[0]] + "." + wordArray[intArr[1]] + "." + wordArray[intArr[2]] + "." + wordArray[intArr[3]];
                }
                catch
                {
                    // do nothing
                }
            }
            return fourWords;
        }

        public static string encHex(byte[] toEncrypt, byte[] seed)
        {
            using var blake3 = Blake3.Hasher.NewKeyed(seed);
            blake3.UpdateWithJoin(toEncrypt);
            var tag = blake3.Finalize();
            byte[] authenticationTag = tag.AsSpan().ToArray();

            string encryptedString = System.BitConverter.ToString(authenticationTag);
            return encryptedString;
        }
    }
}
