﻿# TODO
	* Rework the syncmachine system to support sync skipping and be more friendly with order in the sync process.
	* Different implementations of protocols (377)
	* Test setting dialogID to a level up interface, and instead of setting up the config interface of the interface to print a "Congrats you leveld up" type of deal, we just pushMessage (sendsystemchatmessage) the config message to the client.
  		
	* Undocumented packets:
		* 23
		* 35
		* 57
		* 153
		* 228
		* 40
		* 75
		* 156
		* 181
		* 136 (follow prefix?)
	
	* Verify:
		* 192 (item -> object)
		* 25 (item -> floor item)
		* 236 (pickup ground item)
		* 122 (item option 1)
		* 185 (button click)
		* 155 (npc action 1)
		* 129 (bank all)
		* 16  (item alt opt 2)
		* 135 (bank n of item)
		* 53 (item on item)
		* 87 (drop item)
		* 117 (bank 5)
		* 73 (trade request)
		* 79 (light item) (ground? inv?)
		* 145 (unequip? alt item action?)
		* 17 (npc action 2)
		* 21 (npc action 3)
		* 131 (magic on npc)
		* 252 (object action 2)
		* 72 (attack npc)
		* 249 (magic on player)
		* 39 (trade anwser)
		* 43 (bank 10)
		* 237 (magic on item in inventory)
		* 14 (item on player)
		* 41 (equip item)
		* 18 (npc action 4)
		* 70 (object action 3)
		* 234 (object action 2)
		* 132 (object action 1)
		* 253 (ground item action)
		
		* 188 add friend
		* 133 add ignore
		* 215 del friend
		*  74 del ignore