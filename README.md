# CrewQ
(KSP Mod) Crew Rotation and Variety

Major Goals
* Replace default oldest-first crew selection criteria
* Make sure that crew roles are well-represented in the 'default' pod, in order to assist in mission success. Different pod types will have different priority structures. One of each will be chosen (if available), and any extra will be chosen pesudo-randomly.
  * Command or Atmospheric (Mk1, Mk2, Mk3 Cockpits, Mk1, Mk1-2 capsules, similar mod parts, unconfigured mod parts); Pilot, Engineer, Scientist
  * Orbital (Cupola, Hitchhiker, similar mod parts); Engineer, Scientist
  * Science Lab (Mobile Laboratory, similar mod parts); Scientist
  * Lander Cans (Mk1, Mk2 lander-cans, similar mod parts); Pilot, Engineer, Scientist
* Apply a 'vacation period' after each mission for participants (10% of mission duration, or 30 days, whichever is greater (configurable)) where Crew Member will not be automatically selected unless no other Crew are available.
* First Crew Member of each Profession will be the highest Rank available at the Space Center & not on vacation. Additional crew members will be chosen pesudo-randomly from non-vacationing Crew Members, with a bias toward the lowest-ranked.
* 'Stock-alike' feel. 

Minor Goals/Medium-Term features
* Option to allow the 'vacation period' to be a *hard lock*, preventing assignment of that Crew Member.
* Editor GUI to allow on-the-fly restructuring of crew priorities on a per-part basis.
