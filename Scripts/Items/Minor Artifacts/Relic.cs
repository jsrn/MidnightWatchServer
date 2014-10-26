using System;
using Server;

namespace Server.Items
{
	public class Relic : Item
	{
		public override int LabelNumber{ get{ return 1063489; } }
		
		[Constructable]
		public Relic() : base( 0x1BEB )
		{
			SetName();
			SetItemID();
		}

		private void SetName()
		{
			string[] adjectives = {
				"Glistening",
				"Flawless",
				"Beautiful",
				"Menacing",
				"Spiked",
				"Fearsome",
				"Dire"
			};
			string[] nouns      = {
				"bauble",
				"trinket",
				"relic",
				"artifact"
			};
			string[] sets       = {
				"Mondain",
				"Minax",
				"the Shadowlords",
				"the Council of Mages",
				"the True Britannians"
			};

			Random rnd = new Random();

			string name = adjectives[rnd.Next(adjectives.Length)];
			name += " ";
			name += nouns[rnd.Next(nouns.Length)];
			name += " of ";
			name += sets[rnd.Next(sets.Length)];

			Name = name;
		}

		private void SetItemID()
		{
			int[] itemIDs = {
				0x1BEB, 2886, 2887, 2888,
				3570, 3571, 3572, 3573,
				3629, 3630, 3631, 3632,
				4091, 4092, 4093, 4094,
				4810, 4811,
				7185, 7186, 7187,
				7960,
				9244, 9245, 9246,
				9442,
				10310, 10311,
				11694, 11695
			};

			Random rnd = new Random();
			ItemID = itemIDs[rnd.Next(itemIDs.Length)];
		}

		public Relic( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}