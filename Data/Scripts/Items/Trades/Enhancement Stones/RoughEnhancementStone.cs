using System;
using Server;
using Server.Targeting;

namespace Server.Items
{
	public class RoughEnhancementStone : Item
	{
		private int i_Uses;
		[CommandProperty( AccessLevel.GameMaster )]
		public int Uses { get { return i_Uses; } set { i_Uses = value; InvalidateProperties(); } }

		[Constructable] 
		public RoughEnhancementStone() : this( 5 )
		{
		}

		[Constructable] 
		public RoughEnhancementStone( int uses ) : base( 0x1F14 ) 
		{ 
			Weight = 1.0;
			i_Uses = uses;
			Hue = 0x38C;
			Name = "Channeling Enhancement Stone";
		} 

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			list.Add( 1060584, "{0}\t{1}", i_Uses.ToString(), "Uses" );
		}
		
		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				if ( Uses < 1 )
				{
					Delete();
					from.SendMessage(32, "This have no charges so it's gone!");
				}
				from.SendMessage("Which weapon or shield you want to try to enhance?");
				from.Target = new RoughEnhancementStoneTarget(this);
			}
			else
				from.SendMessage("This must be in your backpack to use.");
		}
		
        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			list.Add( 1070722, "Add Spell Channeling to any weapon or shield");
        }

		public void Enhancement(Mobile from, object o)
		{
			if ( o is Item )
			{
				if ( !((Item)o).IsChildOf( from.Backpack ) )
				{
					from.SendMessage(32, "This must be in your backpack to enhance");
				}
				else if (o is BaseWeapon && ((BaseWeapon)o).IsChildOf(from.Backpack))
				{
					BaseWeapon weap = o as BaseWeapon;
					if (weap.Attributes.SpellChanneling == 1)
					{
						from.SendMessage(32, "This weapon already has Spell Channeling");
						return;
					}
					else if (from.Skills[SkillName.Blacksmith].Value < 50.0)
						from.SendMessage(32, "You need at least 50.0 blacksmith and magery to enhance weapons with Spell Channeling");
					else if (from.Skills[SkillName.Magery].Value < 50.0)
						from.SendMessage(32, "You need at least 50.0 blacksmith and magery to enhance weapons with Spell Channeling");
					else if ( !Deleted )
					{
						if (weap.Attributes.SpellChanneling != 1)
							weap.Attributes.CastSpeed -= 1;
							weap.Attributes.SpellChanneling = 1;
					
						if (Uses <= 1)
						{
							from.SendMessage(32, "You used up the enhancement stone");
							Delete();
						}
						else
						{
							--Uses;
							from.SendMessage(32, "You have {0} uses left", Uses);
						}
					}
				}
				else if (o is BaseShield && ((BaseShield)o).IsChildOf(from.Backpack))
				{
					BaseShield shield = o as BaseShield;
					if (shield.Attributes.SpellChanneling == 1)
					{
						from.SendMessage(32, "This shield already has Spell Channeling");
						return;
					}
					else if (from.Skills[SkillName.Blacksmith].Value < 50.0)
						from.SendMessage(32, "You need at least 50.0 blacksmith and magery to enhance shields with Spell Channeling");
					else if (from.Skills[SkillName.Magery].Value < 50.0)
						from.SendMessage(32, "You need at least 50.0 blacksmith and magery to enhance shields with Spell Channeling");
					else if ( !Deleted )
					{
						if (shield.Attributes.SpellChanneling != 1)
							shield.Attributes.CastSpeed -= 1;
							shield.Attributes.SpellChanneling = 1;
					
						if (Uses <= 1)
						{
							from.SendMessage(32, "You used up the enhancement stone");
							Delete();
						}
						else
						{
							--Uses;
							from.SendMessage(32, "You have {0} uses left", Uses);
						}
					}
				}
				else
				{
					from.SendMessage(32, "You cannot enhance that item.");
				}
			}
			else
			{
				from.SendMessage(32, "You cannot enhance that.");
			}
		}

		public RoughEnhancementStone( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( (int) i_Uses );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			i_Uses = reader.ReadInt();
			if ( version == 0 ) { Serial sr_Owner = reader.ReadInt(); }
		}
	}

	public class RoughEnhancementStoneTarget : Target
	{
		private RoughEnhancementStone sb_Weapon;

		public RoughEnhancementStoneTarget(RoughEnhancementStone weapon) : base( 18, false, TargetFlags.None )
		{
			sb_Weapon = weapon;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (sb_Weapon.Deleted)
				return;

			sb_Weapon.Enhancement(from, targeted);
		}
	}
}