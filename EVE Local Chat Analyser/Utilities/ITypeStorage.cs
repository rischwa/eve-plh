using System.Collections.Generic;
using System.Windows.Automation;

namespace EveLocalChatAnalyser.Utilities
{
    //public class ShipTypes
    //{
    //    private static readonly Dictionary<int, string> SHIP_TYPES_BY_ID = new Dictionary<int, string>
    //                                                                           {
    //                                                                               {582, "Bantam"},
    //                                                                               {583, "Condor"},
    //                                                                               {584, "Griffin"},
    //                                                                               {585, "Slasher"},
    //                                                                               {586, "Probe"},
    //                                                                               {587, "Rifter"},
    //                                                                               {588, "Reaper"},
    //                                                                               {589, "Executioner"},
    //                                                                               {590, "Inquisitor"},
    //                                                                               {591, "Tormentor"},
    //                                                                               {592, "Navitas"},
    //                                                                               {593, "Tristan"},
    //                                                                               {594, "Incursus"},
    //                                                                               {595, "Gallente Police Ship"},
    //                                                                               {596, "Impairor"},
    //                                                                               {597, "Punisher"},
    //                                                                               {598, "Breacher"},
    //                                                                               {599, "Burst"},
    //                                                                               {
    //                                                                                   600, "Minmatar Peacekeeper Ship"
    //                                                                               },
    //                                                                               {601, "Ibis"},
    //                                                                               {602, "Kestrel"},
    //                                                                               {603, "Merlin"},
    //                                                                               {605, "Heron"},
    //                                                                               {606, "Velator"},
    //                                                                               {607, "Imicus"},
    //                                                                               {608, "Atron"},
    //                                                                               {609, "Maulus"},
    //                                                                               {613, "Devourer"},
    //                                                                               {614, "Fury"},
    //                                                                               {615, "Immolator"},
    //                                                                               {616, "Medusa"},
    //                                                                               {617, "Echo"},
    //                                                                               {618, "Lynx"},
    //                                                                               {619, "Swordspine"},
    //                                                                               {620, "Osprey"},
    //                                                                               {621, "Caracal"},
    //                                                                               {622, "Stabber"},
    //                                                                               {623, "Moa"},
    //                                                                               {624, "Maller"},
    //                                                                               {625, "Augoror"},
    //                                                                               {626, "Vexor"},
    //                                                                               {627, "Thorax"},
    //                                                                               {628, "Arbitrator"},
    //                                                                               {629, "Rupture"},
    //                                                                               {630, "Bellicose"},
    //                                                                               {631, "Scythe"},
    //                                                                               {632, "Blackbird"},
    //                                                                               {633, "Celestis"},
    //                                                                               {634, "Exequror"},
    //                                                                               {635, "Opux Luxury Yacht"},
    //                                                                               {638, "Raven"},
    //                                                                               {639, "Tempest"},
    //                                                                               {640, "Scorpion"},
    //                                                                               {641, "Megathron"},
    //                                                                               {642, "Apocalypse"},
    //                                                                               {643, "Armageddon"},
    //                                                                               {644, "Typhoon"},
    //                                                                               {645, "Dominix"},
    //                                                                               {648, "Badger"},
    //                                                                               {649, "Tayra"},
    //                                                                               {650, "Nereus"},
    //                                                                               {651, "Hoarder"},
    //                                                                               {652, "Mammoth"},
    //                                                                               {653, "Wreathe"},
    //                                                                               {654, "Kryos"},
    //                                                                               {655, "Epithal"},
    //                                                                               {656, "Miasmos"},
    //                                                                               {657, "Iteron Mark V"},
    //                                                                               {670, "Capsule"},
    //                                                                               {671, "Erebus"},
    //                                                                               {672, "Caldari Shuttle"},
    //                                                                               {1233, "Polaris Enigma Frigate"},
    //                                                                               {1944, "Bestower"},
    //                                                                               {2006, "Omen"},
    //                                                                               {2161, "Crucifier"},
    //                                                                               {2834, "Utu"},
    //                                                                               {2836, "Adrestia"},
    //                                                                               {2863, "Primae"},
    //                                                                               {2998, "Noctis"},
    //                                                                               {3514, "Revenant"},
    //                                                                               {3516, "Malice"},
    //                                                                               {3518, "Vangel"},
    //                                                                               {3532, "Echelon"},
    //                                                                               {3628, "Nation"},
    //                                                                               {3751, "SOCT 1"},
    //                                                                               {3753, "SOCT 2"},
    //                                                                               {3756, "Gnosis"},
    //                                                                               {3764, "Leviathan"},
    //                                                                               {3766, "Vigil"},
    //                                                                               {3768, "Amarr Police Frigate"},
    //                                                                               {4005, "Scorpion Ishukone Watch"},
    //                                                                               {4302, "Oracle"},
    //                                                                               {4306, "Naga"},
    //                                                                               {4308, "Talos"},
    //                                                                               {4310, "Tornado"},
    //                                                                               {
    //                                                                                   4363,
    //                                                                                   "Miasmos Quafe Ultra Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   4388,
    //                                                                                   "Miasmos Quafe Ultramarine Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   9854,
    //                                                                                   "Polaris Inspector Frigate"
    //                                                                               },
    //                                                                               {9858, "Polaris Centurion TEST"},
    //                                                                               {9860, "Polaris Legatus Frigate"},
    //                                                                               {
    //                                                                                   9862,
    //                                                                                   "Polaris Centurion Frigate"
    //                                                                               },
    //                                                                               {11011, "Guardian-Vexor"},
    //                                                                               {11019, "Cockroach"},
    //                                                                               {11129, "Gallente Shuttle"},
    //                                                                               {11132, "Minmatar Shuttle"},
    //                                                                               {11134, "Amarr Shuttle"},
    //                                                                               {11172, "Helios"},
    //                                                                               {11174, "Keres"},
    //                                                                               {11176, "Crow"},
    //                                                                               {11178, "Raptor"},
    //                                                                               {11182, "Cheetah"},
    //                                                                               {11184, "Crusader"},
    //                                                                               {11186, "Malediction"},
    //                                                                               {11188, "Anathema"},
    //                                                                               {11190, "Sentinel"},
    //                                                                               {11192, "Buzzard"},
    //                                                                               {11194, "Kitsune"},
    //                                                                               {11196, "Claw"},
    //                                                                               {11198, "Stiletto"},
    //                                                                               {11200, "Taranis"},
    //                                                                               {11202, "Ares"},
    //                                                                               {11365, "Vengeance"},
    //                                                                               {11371, "Wolf"},
    //                                                                               {11373, "Blade"},
    //                                                                               {11375, "Erinye"},
    //                                                                               {11377, "Nemesis"},
    //                                                                               {11379, "Hawk"},
    //                                                                               {11381, "Harpy"},
    //                                                                               {11383, "Gatherer"},
    //                                                                               {11387, "Hyena"},
    //                                                                               {11389, "Kishar"},
    //                                                                               {11393, "Retribution"},
    //                                                                               {11400, "Jaguar"},
    //                                                                               {11567, "Avatar"},
    //                                                                               {
    //                                                                                   11936,
    //                                                                                   "Apocalypse Imperial Issue"
    //                                                                               },
    //                                                                               {
    //                                                                                   11938,
    //                                                                                   "Armageddon Imperial Issue"
    //                                                                               },
    //                                                                               {11940, "Gold Magnate"},
    //                                                                               {11942, "Silver Magnate"},
    //                                                                               {11957, "Falcon"},
    //                                                                               {11959, "Rook"},
    //                                                                               {11961, "Huginn"},
    //                                                                               {11963, "Rapier"},
    //                                                                               {11965, "Pilgrim"},
    //                                                                               {11969, "Arazu"},
    //                                                                               {11971, "Lachesis"},
    //                                                                               {11978, "Scimitar"},
    //                                                                               {11985, "Basilisk"},
    //                                                                               {11987, "Guardian"},
    //                                                                               {11989, "Oneiros"},
    //                                                                               {11993, "Cerberus"},
    //                                                                               {11995, "Onyx"},
    //                                                                               {11999, "Vagabond"},
    //                                                                               {12003, "Zealot"},
    //                                                                               {12005, "Ishtar"},
    //                                                                               {12011, "Eagle"},
    //                                                                               {12013, "Broadsword"},
    //                                                                               {12015, "Muninn"},
    //                                                                               {12017, "Devoter"},
    //                                                                               {12019, "Sacrilege"},
    //                                                                               {12021, "Phobos"},
    //                                                                               {12023, "Deimos"},
    //                                                                               {12032, "Manticore"},
    //                                                                               {12034, "Hound"},
    //                                                                               {12036, "Dagger"},
    //                                                                               {12038, "Purifier"},
    //                                                                               {12042, "Ishkur"},
    //                                                                               {12044, "Enyo"},
    //                                                                               {12729, "Crane"},
    //                                                                               {12731, "Bustard"},
    //                                                                               {12733, "Prorator"},
    //                                                                               {12735, "Prowler"},
    //                                                                               {12743, "Viator"},
    //                                                                               {12745, "Occator"},
    //                                                                               {12747, "Mastodon"},
    //                                                                               {12753, "Impel"},
    //                                                                               {
    //                                                                                   13202,
    //                                                                                   "Megathron Federate Issue"
    //                                                                               },
    //                                                                               {16227, "Ferox"},
    //                                                                               {16229, "Brutix"},
    //                                                                               {16231, "Cyclone"},
    //                                                                               {16233, "Prophecy"},
    //                                                                               {16236, "Coercer"},
    //                                                                               {16238, "Cormorant"},
    //                                                                               {16240, "Catalyst"},
    //                                                                               {16242, "Thrasher"},
    //                                                                               {17360, "Immovable Enigma"},
    //                                                                               {17476, "Covetor"},
    //                                                                               {17478, "Retriever"},
    //                                                                               {17480, "Procurer"},
    //                                                                               {17619, "Caldari Navy Hookbill"},
    //                                                                               {17634, "Caracal Navy Issue"},
    //                                                                               {17636, "Raven Navy Issue"},
    //                                                                               {17703, "Imperial Navy Slicer"},
    //                                                                               {17705, "Khanid Navy Frigate"},
    //                                                                               {17707, "Mordus Frigate"},
    //                                                                               {17709, "Omen Navy Issue"},
    //                                                                               {17713, "Stabber Fleet Issue"},
    //                                                                               {17715, "Gila"},
    //                                                                               {17718, "Phantasm"},
    //                                                                               {17720, "Cynabal"},
    //                                                                               {17722, "Vigilant"},
    //                                                                               {17726, "Apocalypse Navy Issue"},
    //                                                                               {17728, "Megathron Navy Issue"},
    //                                                                               {17732, "Tempest Fleet Issue"},
    //                                                                               {17736, "Nightmare"},
    //                                                                               {17738, "Machariel"},
    //                                                                               {17740, "Vindicator"},
    //                                                                               {
    //                                                                                   17812, "Republic Fleet Firetail"
    //                                                                               },
    //                                                                               {17841, "Federation Navy Comet"},
    //                                                                               {17843, "Vexor Navy Issue"},
    //                                                                               {17918, "Rattlesnake"},
    //                                                                               {17920, "Bhaalgorn"},
    //                                                                               {17922, "Ashimmu"},
    //                                                                               {17924, "Succubus"},
    //                                                                               {17926, "Cruor"},
    //                                                                               {17928, "Daredevil"},
    //                                                                               {17930, "Worm"},
    //                                                                               {17932, "Dramiel"},
    //                                                                               {19720, "Revelation"},
    //                                                                               {19722, "Naglfar"},
    //                                                                               {19724, "Moros"},
    //                                                                               {19726, "Phoenix"},
    //                                                                               {19744, "Sigil"},
    //                                                                               {20125, "Curse"},
    //                                                                               {20183, "Providence"},
    //                                                                               {20185, "Charon"},
    //                                                                               {20187, "Obelisk"},
    //                                                                               {20189, "Fenrir"},
    //                                                                               {21097, "Goru's Shuttle"},
    //                                                                               {21628, "Guristas Shuttle"},
    //                                                                               {22428, "Redeemer"},
    //                                                                               {22430, "Sin"},
    //                                                                               {22436, "Widow"},
    //                                                                               {22440, "Panther"},
    //                                                                               {22442, "Eos"},
    //                                                                               {22444, "Sleipnir"},
    //                                                                               {22446, "Vulture"},
    //                                                                               {22448, "Absolution"},
    //                                                                               {22452, "Heretic"},
    //                                                                               {22456, "Sabre"},
    //                                                                               {22460, "Eris"},
    //                                                                               {22464, "Flycatcher"},
    //                                                                               {22466, "Astarte"},
    //                                                                               {22468, "Claymore"},
    //                                                                               {22470, "Nighthawk"},
    //                                                                               {22474, "Damnation"},
    //                                                                               {22544, "Hulk"},
    //                                                                               {22546, "Skiff"},
    //                                                                               {22548, "Mackinaw"},
    //                                                                               {22852, "Hel"},
    //                                                                               {23757, "Archon"},
    //                                                                               {23773, "Ragnarok"},
    //                                                                               {23911, "Thanatos"},
    //                                                                               {23913, "Nyx"},
    //                                                                               {23915, "Chimera"},
    //                                                                               {23917, "Wyvern"},
    //                                                                               {23919, "Aeon"},
    //                                                                               {24483, "Nidhoggur"},
    //                                                                               {24688, "Rokh"},
    //                                                                               {24690, "Hyperion"},
    //                                                                               {24692, "Abaddon"},
    //                                                                               {24694, "Maelstrom"},
    //                                                                               {24696, "Harbinger"},
    //                                                                               {24698, "Drake"},
    //                                                                               {24700, "Myrmidon"},
    //                                                                               {24702, "Hurricane"},
    //                                                                               {25560, "Opux Dragoon Yacht"},
    //                                                                               {26840, "Raven State Issue"},
    //                                                                               {26842, "Tempest Tribal Issue"},
    //                                                                               {27299, "Civilian Amarr Shuttle"},
    //                                                                               {
    //                                                                                   27301,
    //                                                                                   "Civilian Caldari Shuttle"
    //                                                                               },
    //                                                                               {
    //                                                                                   27303,
    //                                                                                   "Civilian Gallente Shuttle"
    //                                                                               },
    //                                                                               {
    //                                                                                   27305,
    //                                                                                   "Civilian Minmatar Shuttle"
    //                                                                               },
    //                                                                               {28352, "Rorqual"},
    //                                                                               {28606, "Orca"},
    //                                                                               {28659, "Paladin"},
    //                                                                               {28661, "Kronos"},
    //                                                                               {28665, "Vargur"},
    //                                                                               {28710, "Golem"},
    //                                                                               {28844, "Rhea"},
    //                                                                               {28846, "Nomad"},
    //                                                                               {28848, "Anshar"},
    //                                                                               {28850, "Ark"},
    //                                                                               {29248, "Magnate"},
    //                                                                               {29266, "Apotheosis"},
    //                                                                               {29328, "Amarr Media Shuttle"},
    //                                                                               {29330, "Caldari Media Shuttle"},
    //                                                                               {29332, "Gallente Media Shuttle"},
    //                                                                               {29334, "Minmatar Media Shuttle"},
    //                                                                               {29336, "Scythe Fleet Issue"},
    //                                                                               {29337, "Augoror Navy Issue"},
    //                                                                               {29340, "Osprey Navy Issue"},
    //                                                                               {29344, "Exequror Navy Issue"},
    //                                                                               {29984, "Tengu"},
    //                                                                               {29986, "Legion"},
    //                                                                               {29988, "Proteus"},
    //                                                                               {29990, "Loki"},
    //                                                                               {30842, "Interbus Shuttle"},
    //                                                                               {32207, "Freki"},
    //                                                                               {32209, "Mimir"},
    //                                                                               {32305, "Armageddon Navy Issue"},
    //                                                                               {32307, "Dominix Navy Issue"},
    //                                                                               {32309, "Scorpion Navy Issue"},
    //                                                                               {32311, "Typhoon Fleet Issue"},
    //                                                                               {32788, "Cambion"},
    //                                                                               {32790, "Etana"},
    //                                                                               {
    //                                                                                   32811,
    //                                                                                   "Miasmos Amastris Edition"
    //                                                                               },
    //                                                                               {32840, "InterBus Catalyst"},
    //                                                                               {
    //                                                                                   32842,
    //                                                                                   "Intaki Syndicate Catalyst"
    //                                                                               },
    //                                                                               {
    //                                                                                   32844,
    //                                                                                   "Inner Zone Shipping Catalyst"
    //                                                                               },
    //                                                                               {32846, "Quafe Catalyst"},
    //                                                                               {32848, "Aliastra Catalyst"},
    //                                                                               {32872, "Algos"},
    //                                                                               {32874, "Dragoon"},
    //                                                                               {32876, "Corax"},
    //                                                                               {32878, "Talwar"},
    //                                                                               {32880, "Venture"},
    //                                                                               {32983, "Sukuuvestaa Heron"},
    //                                                                               {
    //                                                                                   32985,
    //                                                                                   "Inner Zone Shipping Imicus"
    //                                                                               },
    //                                                                               {32987, "Sarum Magnate"},
    //                                                                               {32989, "Vherokior Probe"},
    //                                                                               {33079, "Hematos"},
    //                                                                               {33081, "Taipan"},
    //                                                                               {33083, "Violator"},
    //                                                                               {33099, "Nefantar Thrasher"},
    //                                                                               {33151, "Brutix Navy Issue"},
    //                                                                               {33153, "Drake Navy Issue"},
    //                                                                               {33155, "Harbinger Navy Issue"},
    //                                                                               {33157, "Hurricane Fleet Issue"},
    //                                                                               {33190, "Tash-Murkon Magnate"},
    //                                                                               {
    //                                                                                   33328,
    //                                                                                   "Capsule - Genolution 'Auroral' 197-variant"
    //                                                                               },
    //                                                                               {33395, "Moracha"},
    //                                                                               {33397, "Chremoas"},
    //                                                                               {33468, "Astero"},
    //                                                                               {33470, "Stratios"},
    //                                                                               {33472, "Nestor"},
    //                                                                               {33513, "Leopard"},
    //                                                                               {
    //                                                                                   33553,
    //                                                                                   "Stratios Emergency Responder"
    //                                                                               },
    //                                                                               {
    //                                                                                   33623,
    //                                                                                   "Abaddon Tash-Murkon Edition"
    //                                                                               },
    //                                                                               {33625, "Abaddon Kador Edition"},
    //                                                                               {
    //                                                                                   33627, "Rokh Nugoeihuvi Edition"
    //                                                                               },
    //                                                                               {33629, "Rokh Wiyrkomi Edition"},
    //                                                                               {
    //                                                                                   33631,
    //                                                                                   "Maelstrom Nefantar Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33633,
    //                                                                                   "Maelstrom Krusual Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33635,
    //                                                                                   "Hyperion Aliastra Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33637,
    //                                                                                   "Hyperion Innerzone Shipping Edition"
    //                                                                               },
    //                                                                               {33639, "Omen Kador Edition"},
    //                                                                               {
    //                                                                                   33641,
    //                                                                                   "Omen Tash-Murkon Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33643,
    //                                                                                   "Caracal Nugoeihuvi Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33645,
    //                                                                                   "Caracal Wiyrkomi Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33647,
    //                                                                                   "Stabber Nefantar Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33649, "Stabber Krusual Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33651, "Thorax Aliastra Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33653,
    //                                                                                   "Thorax Innerzone Shipping Edition"
    //                                                                               },
    //                                                                               {33655, "Punisher Kador Edition"},
    //                                                                               {
    //                                                                                   33657,
    //                                                                                   "Punisher Tash-Murkon Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33659,
    //                                                                                   "Merlin Nugoeihuvi Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33661, "Merlin Wiyrkomi Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33663, "Rifter Nefantar Edition"
    //                                                                               },
    //                                                                               {33665, "Rifter Krusual Edition"},
    //                                                                               {
    //                                                                                   33667,
    //                                                                                   "Incursus Aliastra Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33669,
    //                                                                                   "Incursus Innerzone Shipping Edition"
    //                                                                               },
    //                                                                               {33673, "Whiptail"},
    //                                                                               {33675, "Chameleon"},
    //                                                                               {33677, "Police Pursuit Comet"},
    //                                                                               {
    //                                                                                   33683,
    //                                                                                   "Mackinaw ORE Development Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33685,
    //                                                                                   "Orca ORE Development Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33687,
    //                                                                                   "Rorqual ORE Development Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33689,
    //                                                                                   "Iteron Inner Zone Shipping Edition"
    //                                                                               },
    //                                                                               {33691, "Tayra Wiyrkomi Edition"},
    //                                                                               {
    //                                                                                   33693,
    //                                                                                   "Mammoth Nefantar Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33695,
    //                                                                                   "Bestower Tash-Murkon Edition"
    //                                                                               },
    //                                                                               {33697, "Prospect"},
    //                                                                               {33816, "Garmur"},
    //                                                                               {33818, "Orthrus"},
    //                                                                               {33820, "Barghest"},
    //                                                                               {
    //                                                                                   33869,
    //                                                                                   "Brutix Serpentis Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33871,
    //                                                                                   "Cyclone Thukker Tribe Edition"
    //                                                                               },
    //                                                                               {33873, "Ferox Guristas Edition"},
    //                                                                               {
    //                                                                                   33875,
    //                                                                                   "Prophecy Blood Raiders Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33877,
    //                                                                                   "Catalyst Serpentis Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33879,
    //                                                                                   "Coercer Blood Raiders Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33881,
    //                                                                                   "Cormorant Guristas Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   33883,
    //                                                                                   "Thrasher Thukker Tribe Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34118, "Megathron Quafe Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34151,
    //                                                                                   "Rattlesnake Victory Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34213,
    //                                                                                   "Apocalypse Blood Raider Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34215,
    //                                                                                   "Apocalypse Kador Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34217,
    //                                                                                   "Apocalypse Tash-Murkon Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34219,
    //                                                                                   "Paladin Blood Raider Edition"
    //                                                                               },
    //                                                                               {34221, "Paladin Kador Edition"},
    //                                                                               {
    //                                                                                   34223,
    //                                                                                   "Paladin Tash-Murkon Edition"
    //                                                                               },
    //                                                                               {34225, "Raven Guristas Edition"},
    //                                                                               {
    //                                                                                   34227,
    //                                                                                   "Raven Kaalakiota Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34229,
    //                                                                                   "Raven Nugoeihuvi Edition"
    //                                                                               },
    //                                                                               {34231, "Golem Guristas Edition"},
    //                                                                               {
    //                                                                                   34233,
    //                                                                                   "Golem Kaalakiota Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34235,
    //                                                                                   "Golem Nugoeihuvi Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34237,
    //                                                                                   "Megathron Police Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34239,
    //                                                                                   "Megathron Innerzone Shipping Edition"
    //                                                                               },
    //                                                                               {34241, "Kronos Police Edition"},
    //                                                                               {34243, "Kronos Quafe Edition"},
    //                                                                               {
    //                                                                                   34245,
    //                                                                                   "Kronos Innerzone Shipping Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34247, "Tempest Justice Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34249, "Tempest Krusual Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34251,
    //                                                                                   "Tempest Nefantar Edition"
    //                                                                               },
    //                                                                               {34253, "Vargur Justice Edition"},
    //                                                                               {34255, "Vargur Krusual Edition"},
    //                                                                               {
    //                                                                                   34257, "Vargur Nefantar Edition"
    //                                                                               },
    //                                                                               {34317, "Confessor"},
    //                                                                               {34328, "Bowhead"},
    //                                                                               {34339, "Moros Interbus Edition"},
    //                                                                               {
    //                                                                                   34341, "Naglfar Justice Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34343,
    //                                                                                   "Phoenix Wiyrkomi Edition"
    //                                                                               },
    //                                                                               {
    //                                                                                   34345,
    //                                                                                   "Revelation Sarum Edition"
    //                                                                               },
    //        {34562, "Svipul" }
    //                                                                           };

    //    //public static HashSet<string> ShipTypeNames;

    //    //static ShipTypes()
    //    //{
    //    //    ShipTypeNames = new HashSet<string>(SHIP_TYPES_BY_ID.Values);
    //    //}

    //    //public static string GetShipTypeName(int id)
    //    //{
    //    //    string name;
    //    //    return SHIP_TYPES_BY_ID.TryGetValue(id, out name) ? name : ("unknown: " + id);
    //    //}

    //    public static bool IsPod(int shipTypeID)
    //    {
    //        return shipTypeID == 670 || shipTypeID == 33328;

    //    }
    //}

    public interface ITypeStorage
    {
        IEnumerable<TypeInfo> ShipTypes { get; }

        void SetTypes(IEnumerable<TypeInfo> types);
    }
}
