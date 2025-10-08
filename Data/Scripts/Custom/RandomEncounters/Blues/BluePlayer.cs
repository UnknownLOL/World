using System; 
using Server;
using System.Collections; 
using Server.Misc; 
using Server.Items; 
using Server.Mobiles; 

namespace Server.Mobiles 
{ 
	public class BluePlayer : BaseBlue
	{ 
		public int CitizenType;
		[CommandProperty(AccessLevel.Owner)]
		public int Citizen_Type { get { return CitizenType; } set { CitizenType = value; InvalidateProperties(); } }

		public int CitizenLevel;
		[CommandProperty(AccessLevel.Owner)]
		public int Citizen_Level { get { return CitizenLevel; } set { CitizenLevel = value; InvalidateProperties(); } }

		private bool m_Bandaging;
		public static TimeSpan TalkDelay = TimeSpan.FromSeconds( 30.0 );
		public DateTime m_NextTalk;

		[Constructable] 
		public BluePlayer() : base( AIType.AI_Melee, FightMode.Closest, 25, 1, 0.4, 0.3 ) 
		{ 

			NameHue = 1993;

			if ( Female = Utility.RandomBool() ) 
			{ 
				Body = 401; 
				Name = NameList.RandomName( "female" );
			}
			else 
			{ 
				Body = 400; 			
				Name = NameList.RandomName( "male" ); 
				FacialHairItemID = Utility.RandomList( 0, 0, 8254, 8255, 8256, 8257, 8267, 8268, 8269 );
			}

			SetStr( 300 );
			SetDex( 300 );
			SetInt( 300 );

			switch ( Utility.Random( 3 ) )
			{
				case 0: Server.Misc.IntelligentAction.DressUpWizards( this, false ); 					CitizenType = 1;	break;
				case 1: Server.Misc.IntelligentAction.DressUpFighters( this, "", false, false, true );	CitizenType = 2;	break;
				case 2: Server.Misc.IntelligentAction.DressUpRogues( this, "", false, false, true );	CitizenType = 3;	break;
			}

			Title = TavernPatrons.GetTitle();
			Hue = Utility.RandomSkinColor();
			Utility.AssignRandomHair( this );
			SpeechHue = Utility.RandomTalkHue();
			AI = AIType.AI_Citizen;
			HairHue = Utility.RandomHairHue();
			FacialHairHue = HairHue;
			CitizenLevel = Utility.RandomMinMax( 1, 9 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, (CitizenLevel*4), (CitizenLevel*7) );
			SetResistance( ResistanceType.Fire, (CitizenLevel*4), (CitizenLevel*7) );
			SetResistance( ResistanceType.Cold, (CitizenLevel*4), (CitizenLevel*7) );
			SetResistance( ResistanceType.Poison, (CitizenLevel*4), (CitizenLevel*7) );
			SetResistance( ResistanceType.Energy, (CitizenLevel*4), (CitizenLevel*7) );

			if ( CitizenType == 1 )
			{
				AI = AIType.AI_Mage;
				SetStr( (CitizenLevel*50), (CitizenLevel*70) );
				SetDex( (CitizenLevel*70), (CitizenLevel*90) );
				SetInt( (CitizenLevel*100), (CitizenLevel*130) );

				SetHits( (CitizenLevel*100), (CitizenLevel*130) );

				SetSkill( SkillName.Psychology, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Magery, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Meditation, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.MagicResist, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Tactics, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.FistFighting, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Marksmanship, (28+(CitizenLevel*7)) );

				AddRangeWeapon();
			}
			else if ( CitizenType == 2 )
			{
				AI = AIType.AI_Melee;
				SetStr( (CitizenLevel*100), (CitizenLevel*130) );
				SetDex( (CitizenLevel*70), (CitizenLevel*90) );
				SetInt( (CitizenLevel*50), (CitizenLevel*70) );

				SetHits( (CitizenLevel*100), (CitizenLevel*130) );

				SetSkill( SkillName.Fencing, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Bludgeoning, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Swords, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.MagicResist, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Tactics, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.FistFighting, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Marksmanship, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Parry, (28+(CitizenLevel*7)) );
			}
			else
			{
				AI = AIType.AI_Archer;
				SetStr( (CitizenLevel*70), (CitizenLevel*90) );
				SetDex( (CitizenLevel*100), (CitizenLevel*130) );
				SetInt( (CitizenLevel*50), (CitizenLevel*70) );

				SetHits( (CitizenLevel*100), (CitizenLevel*130) );

				SetSkill( SkillName.MagicResist, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Tactics, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.FistFighting, (28+(CitizenLevel*7)) );
				SetSkill( SkillName.Marksmanship, (28+(CitizenLevel*7)) );

				AddRangeWeapon();
			}

			SetDamage( (CitizenLevel*2), (CitizenLevel*3) );

			Fame = 2500 * CitizenLevel;
			Karma = Fame;

			VirtualArmor = CitizenLevel * 10;

			Criminal = false; //Wazi
			Kills = 0;

			for (int i = 0; i < 3; i++)
			{
				PackItem( new GreaterCurePotion() );
				PackItem( new GreaterHealPotion() );
				PackItem( new TotalRefreshPotion() );
			}

			PackItem(new Bandage(Utility.RandomMinMax(10, 100)));
		}

		public override void OnAfterSpawn() //Wazi - no idea why this works
		{
		    base.OnAfterSpawn();

			if ( Utility.RandomBool() && !Server.Misc.Worlds.InBuilding( this ) && this.Map != Map.SerpentIsland )
			{
				BaseMount mount = new EvilMount();

				if ( this.Map == Map.SavagedEmpire )
				{
					mount.Body = 0x11C; mount.ItemID = 0x3E92; mount.Hue = Utility.RandomList( 0xB79, 0xB19, 0xAEF, 0xACE, 0xAB0 );
				}
				else if ( this.Map == Map.IslesDread )
				{
					mount.Body = 23; mount.ItemID = 23;
					if ( Utility.RandomBool() ){ mount.Body = 177; mount.ItemID = 177; }
				}
				else if ( this.Map == Map.Lodor )
				{
					mount.Body = 188; mount.ItemID = 0x3EB8;
					if ( Utility.RandomBool() ){ mount.Body = 0x31F; mount.ItemID = 0x3EBE; }
				}
				else
				{
					mount.Body = 0xE2; mount.ItemID = 594;
				}

				Server.Mobiles.BaseMount.Ride( mount, this );
			}
		}

		public override void GenerateLoot()
		{
			if ( CitizenLevel > 8 ){ AddLoot( LootPack.FilthyRich ); }
			if ( CitizenLevel > 6 ){ AddLoot( LootPack.FilthyRich ); }
			if ( CitizenLevel > 4 ){ AddLoot( LootPack.Rich ); }
			if ( CitizenLevel > 2 ){ AddLoot( LootPack.Average ); }
			AddLoot( LootPack.Meager );
			// adventurers have a 1 in 25 chance of having one rare item in their pack 
			if (Utility.Random(25) == 0)
    		{
    		    Type rareType = Loot.AdventurerRareItemTypes[Utility.Random(Loot.AdventurerRareItemTypes.Length)];
    		    Item rare = Activator.CreateInstance(rareType) as Item;
    		    if (rare != null)
    		        PackItem(rare);
    		}


			if ( CitizenType == 1 ){ AddLoot( LootPack.MedScrolls, ( (int)( ( CitizenLevel / 3 ) + 1 ) ) ); }
		}

		public void AddRangeWeapon()
		{
			if ( FindItemOnLayer( Layer.OneHanded ) != null ) { FindItemOnLayer( Layer.OneHanded ).Delete(); }
			if ( FindItemOnLayer( Layer.TwoHanded ) != null ) { FindItemOnLayer( Layer.TwoHanded ).Delete(); }

			if ( Utility.RandomBool() )
			{
				ThrowingGloves glove = new ThrowingGloves();
				ThrowingWeapon ammo = new ThrowingWeapon( Utility.RandomMinMax( 15, 30 ) );

				switch ( Utility.Random( 5 ))		   
				{
					case 0: glove.GloveType = "Stones";		ammo.ammo = "Throwing Stones"; 	ammo.ItemID = 0x10B6; ammo.Name = "throwing stone";		break;
					case 1: glove.GloveType = "Axes"; 		ammo.ammo = "Throwing Axes"; 	ammo.ItemID = 0x10B3; ammo.Name = "throwing axe";		break;
					case 2: glove.GloveType = "Daggers"; 	ammo.ammo = "Throwing Daggers"; ammo.ItemID = 0x10B7; ammo.Name = "throwing dagger";	break;
					case 3: glove.GloveType = "Darts"; 		ammo.ammo = "Throwing Darts"; 	ammo.ItemID = 0x10B5; ammo.Name = "throwing dart";		break;
					case 4: glove.GloveType = "Stars"; 		ammo.ammo = "Throwing Stars"; 	ammo.ItemID = 0x10B2; ammo.Name = "throwing star";		break;
				};

				AddItem( glove );
				PackItem( ammo );
			}
			else if ( CitizenType == 1 )
			{
				switch ( Utility.Random( 2 ))		   
				{
					case 0: AddItem( new WizardStaff() );		break;
					case 1: AddItem( new WizardStick() );		break;
				};

				PackItem( new MageEye( Utility.RandomMinMax( 15, 30 ) ) );
			}
			else
			{
				switch ( Utility.Random( 8 ))		   
				{
					case 0: AddItem( new Bow() );					PackItem( new Arrow( Utility.RandomMinMax( 15, 30 ) ) );		break;
					case 1: AddItem( new Crossbow() );				PackItem( new Bolt( Utility.RandomMinMax( 15, 30 ) ) );			break;
					case 2: AddItem( new HeavyCrossbow() );			PackItem( new Bolt( Utility.RandomMinMax( 15, 30 ) ) );			break;
					case 3: AddItem( new RepeatingCrossbow() );		PackItem( new Bolt( Utility.RandomMinMax( 15, 30 ) ) );			break;
					case 4: AddItem( new CompositeBow() );			PackItem( new Arrow( Utility.RandomMinMax( 15, 30 ) ) );		break;
					case 5: AddItem( new MagicalShortbow() );		PackItem( new Arrow( Utility.RandomMinMax( 15, 30 ) ) );		break;
					case 6: AddItem( new ElvenCompositeLongbow() );	PackItem( new Arrow( Utility.RandomMinMax( 15, 30 ) ) );		break;
					case 7: AddItem( new Harpoon() );				PackItem( new HarpoonRope( Utility.RandomMinMax( 15, 30 ) ) );	break;
				};
			}
		}

		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool ClickTitle{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
//		public override bool AlwaysAttackable{ get{ return true; } }
		public override int Meat{ get{ return 1; } }
		public override int TreasureMapLevel{ get{ return (int)((CitizenLevel/2)+1); } }
		public override int Skeletal{ get{ return Utility.Random(3); } }
		public override SkeletalType SkeletalType{ get{ return SkeletalType.Brittle; } }


		public override void OnThink()
		{
			base.OnThink();
			
			// Use bandages only if not already bandaging and injured
			if (!m_Bandaging && (Hits < HitsMax || Poisoned))
			{
				Bandage bandage = Backpack.FindItemByType<Bandage>();
				
				if (bandage != null)
				{
					m_Bandaging = true;
					bandage.Consume();
					Timer.DelayCall(TimeSpan.FromSeconds(16), () => m_Bandaging = false);
				}
			}
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			if ( InRange( m, 4 ) && !InRange( oldLocation, 4 ) && InLOS( m ) )
			{
				if ( !m.Frozen && DateTime.Now >= m_NextResurrect && !m.Alive && !m.Criminal )
				{
					m_NextResurrect = DateTime.Now + ResurrectDelay;

					if ( m.Map == null || !m.Map.CanFit( m.Location, 16, false, false ) )
					{
						m.SendLocalizedMessage( 502391 ); // Thou can not be resurrected there!
					}
					else if ( CheckResurrect( m ) )
					{
						OfferResurrection( m );
					}
				}
				else if ( DateTime.Now >= m_NextResurrect && m.Hits < (m.HitsMax/2) && m is PlayerMobile )
				{
					OfferHeal( (PlayerMobile) m );
				}
				else if ( DateTime.Now >= m_NextTalk && m is PlayerMobile ) // check if its time to talk
				{
					m_NextTalk = DateTime.Now + TalkDelay; // set next talk time
                           		switch (Utility.Random(9))
                           		{
										case 0: Say("Hello " + m.Name + " have you come to play?"); break;
                                		case 1: Say("" + m.Name + "?"); break;
                        	     	 	case 2: Say("" + m.Name + " where do you think your going?"); break;
                         	    		case 3: Say("Hey " + m.Name + " this is my spawn"); break;
                         	    		case 4: Say(" Hey " + m.Name + " , did you hear about Moonglow?"); break;
										case 5: Say("" + m.Name + " How are ya?"); break;
                         	    		case 6: Say("To adventure, " + m.Name + "."); break;
                         	    		case 7: Say("" + m.Name + "!!"); break;
										case 8: Say("Hi " + m.Name + "!"); break;
                         	 	}
				
				}
			}

		}

		public override bool CheckResurrect( Mobile m )
		{
			if ( m.Criminal )
			{
				Say("You did something wrong, wait a bit"); // Thou art a criminal.  I shall not resurrect thee.
				return false;
			}
			else if ( m.Kills >= 5 )
			{
				Say("I don't help reds"); // Thou'rt not a decent and good person. I shall not resurrect thee.
				return false;
			}
			else if ( m.Karma < 0 )
			{
				Say("You have bad Karma, but OK."); // Thou hast strayed from the path of virtue, but thou still deservest a second chance.
			}
			Say("An Corp");
			return true;
		}


		public BluePlayer( Serial serial ) : base( serial ) 
		{ 
		} 



		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 

			writer.Write( (int) 0 ); // version 
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 

			int version = reader.ReadInt(); 
		} 
	} 
}   