using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient; 

namespace TableDataGenerator
{
    public partial class Form1 : Form
    {
        const UInt32 WARRIOR_CLASS = 1;
        const UInt32 PALADIN_CLASS = 2;
        const UInt32 MAGE_CLASS = 8;


        public Form1()
        {
            InitializeComponent();
        }
        

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // connect to the database
            MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "server=127.0.0.1;uid=root;" +
                "pwd=root;database=mangoszero;";

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString);
                conn.Open();
                lblConnectionState.Text = "Connected";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                lblConnectionState.Text = "Failed to Connect";
            }


            // set text to show successfull connection

        }


        // ------====== Base Health ======------
        // The function works out the base health
        private int getBaseHealth(MySqlConnection conn, UInt32 iClass, int iCreatureLevel)
        {
            // Generate base health
            MySqlDataReader reader;
            string sqlScript = "";
            MySqlCommand cmd;

            // base health
            int iBaseHealth = 0;
            int iMinHealth = 0;
            double dHealthMultiplier = 0;

            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";

            cmd = new MySqlCommand(sqlScript, conn);

            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                txtResults.Text += "" + iCreatureLevel + " " + reader.GetString("Entry") + " ";

                iMinHealth = reader.GetInt32("MinLevelHealth");
                txtResults.Text += "" + iMinHealth;
                dHealthMultiplier = reader.GetDouble("HealthMultiplier");
                txtResults.Text += " " + dHealthMultiplier;

                iBaseHealth = Convert.ToInt32(iMinHealth / dHealthMultiplier);

            }

            reader.Close();

            return iBaseHealth;
        }


        // ------====== Base Mana ======------
        // The function works out the base mana
        private int getBaseMana(MySqlConnection conn, UInt32 iClass, int iCreatureLevel)
        {
            // Generate base health
            MySqlDataReader reader;
            string sqlScript = "";
            MySqlCommand cmd;

            // base health
            int iBaseMana = 0;
            int iMinMana = 0;
            double dManaMultiplier = 0;

            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";

            cmd = new MySqlCommand(sqlScript, conn);

            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                iMinMana = reader.GetInt32("MinLevelMana");
                dManaMultiplier = reader.GetDouble("ManaMultiplier");

                iBaseMana = Convert.ToInt32(iMinMana / dManaMultiplier);

            }

            reader.Close();

            return iBaseMana;
        }


        // ------====== BASE DAMAGE ======------
        // The method works out the base damage
        private double getBaseDamage(MySqlConnection conn, UInt32 iClass, int iCreatureLevel, int iDamageMultiplier, double dDamageVariance, int iBaseMeleeAttackPower)
        {
            MySqlDataReader reader;
            string sqlScript = "";
            MySqlCommand cmd;

            double dBaseDamage = 0; // this is what we need to worl out
            int iBaseMeleeAttackTime = 0; // MeleeBaseAttackTime in creature_tempplate table
            int iMinMeleeDamage = 0;
            int iMaxMeleeDamage = 0;

            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";

            cmd = new MySqlCommand(sqlScript, conn);

            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                txtResults.Text += "" + iCreatureLevel + " " + reader.GetString("Entry") + " ";

                iDamageMultiplier = reader.GetInt32("DamageMultiplier");

                dDamageVariance = reader.GetDouble("DamageVariance");

                iBaseMeleeAttackPower = reader.GetInt32("MeleeAttackPower");

                iBaseMeleeAttackTime = reader.GetInt32("MeleeBaseAttackTime");

                iMinMeleeDamage = reader.GetInt32("MinMeleeDmg");
                iMaxMeleeDamage = reader.GetInt32("MaxMeleeDmg");


                // REVERSE THE CALCULATION in order to acquire the base damage
                // CalculatedMinMeleeDmg=ROUND(((BaseDamage * Damage Variance) + (Base Melee Attackpower / 14)) * (Base Attack Time/1000)) * Damage Multiplier


                // (Base Damage * Damage Variance)
                double dBaseDamage_x_DamageVariance = 0;
                // (Base Attack Time/1000)
                double dBaseAttackTime_DIV_1000 = 0;
                // (Base Melee Attackpower / 14)
                double iBaseMeleeAttackPower_DIV_14 = 0;
                // ((BaseDamage * Damage Variance) + (Base Melee Attackpower / 14)) * (Base Attack Time/1000)) / (Base Attack Time/1000)
                double dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 = 0;
                // (((BaseDamage * Damage Variance) + (Base Melee Attackpower / 14)) * (Base Attack Time/1000))
                double dTotalOfBracketedCalculations = 0;

                // MinMeleeDmg / DamageMultiplyer = OverallValue
                dTotalOfBracketedCalculations = iMinMeleeDamage / iDamageMultiplier;

                // BaseMeleeAtackTime / 1000 = RightMost
                dBaseAttackTime_DIV_1000 = iBaseMeleeAttackTime / 1000;

                // BaseMeleeAttackPower / 14 = Middle
                iBaseMeleeAttackPower_DIV_14 = iBaseMeleeAttackPower / 14;

                // OverallValue / Right = ((Middle + Left) = Average
                dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 = dTotalOfBracketedCalculations / dBaseAttackTime_DIV_1000;

                dBaseDamage_x_DamageVariance = dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 - iBaseMeleeAttackPower_DIV_14;

                dBaseDamage = dBaseDamage_x_DamageVariance / dDamageVariance;

            }
            reader.Close();

            return dBaseDamage;
        }


        // ------====== BASE RANGED ATTACK POWER ======------
        // The method works out the base ranged attack power
        private int getBaseRangedAttackPower(MySqlConnection conn, UInt32 iClass, int iCreatureLevel, int iDamageMultiplier, double dDamageVariance, double dBaseDamage)
        {
            MySqlDataReader reader;
            string sqlScript = "";
            MySqlCommand cmd;

            int iBaseRangedAttackPower = 0; // this is what we need to worl out
            int iBaseRangedAttackTime = 0; // MeleeBaseAttackTime in creature_tempplate table
            int iMinRangedDamage = 0;
            int iMaxRangedDamage = 0;

            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";

            cmd = new MySqlCommand(sqlScript, conn);

            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
  //              txtResults.Text += "" + iCreatureLevel + " " + reader.GetString("Entry") + " ";

                iDamageMultiplier = reader.GetInt32("DamageMultiplier");

                dDamageVariance = reader.GetDouble("DamageVariance");

                iBaseRangedAttackPower = reader.GetInt32("RangedAttackPower");

                iBaseRangedAttackTime = reader.GetInt32("RangedBaseAttackTime");

                iMinRangedDamage = reader.GetInt32("MinRangedDmg");
                txtResults.Text += "\r\n iMinRangedDamage: " + iMinRangedDamage + " --- \r\n";
                iMaxRangedDamage = reader.GetInt32("MaxRangedDmg");
                txtResults.Text += "\r\n iMaxRangedDamage: " + iMaxRangedDamage + " --- \r\n";


                // REVERSE THE CALCULATION in order to acquire the base damage
                // CalculatedMinRangedDmg=ROUND(((BaseDamage * Damage Variance)+(BaseRangedAttackPower/14))*(Base Ranged Attack Time/1000)) * Damage Multiplier

                // (Base Damage * Damage Variance)
                double dBaseDamage_x_DamageVariance = 0;
                // (Base Attack Time/1000)
                double dBaseRangedAtackTime_DIV_1000 = 0;
                //  (((BaseDamage * Damage Variance) + (Base Ranged Attackpower / 14)) * (Base Ranged Attack Time/1000))
                double dTotalOfBracketedCalculations = 0;

                // (Base Ranged Attackpower / 14)
                double iBaseRangedAttackPower_DIV_14 = 0;

                // ((BaseDamage * Damage Variance) + (Base Melee Attackpower / 14)) * (Base Attack Time/1000)) / (Base Attack Time/1000)
                double dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 = 0;




                // MinRangedDamage / DamageMultiplyer = OverallValue of bracketed calculations
                dTotalOfBracketedCalculations = iMinRangedDamage / iDamageMultiplier;
                txtResults.Text += "\r\n dTotalOfBracketedCalculations: " + dTotalOfBracketedCalculations + " --- \r\n";

                // BaseRangedAttackTime / 1000 = RightMost
                dBaseRangedAtackTime_DIV_1000 = iBaseRangedAttackTime / 1000;
                txtResults.Text += "\r\n dBaseRangedAtackTime_DIV_1000: " + dBaseRangedAtackTime_DIV_1000 + " --- \r\n";



                // OverallValue / Right = ((Middle + Left) = Average
                dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 = dTotalOfBracketedCalculations / dBaseRangedAtackTime_DIV_1000;
                txtResults.Text += "\r\n dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000: " + dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 + " --- \r\n\r\n";


                // BaseDamageExp? * Creature_Template.DamageVariance
                dBaseDamage_x_DamageVariance = dBaseDamage * dDamageVariance;
                txtResults.Text += "\r\n dBaseDamage_x_DamageVariance: " + dBaseDamage_x_DamageVariance + " --- \r\n";

                iBaseRangedAttackPower_DIV_14 = dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 - dBaseDamage_x_DamageVariance;
                txtResults.Text += "r\n iBaseRangedAttackPower_DIV_14: " + iBaseRangedAttackPower_DIV_14 + " --- \r\n";

                iBaseRangedAttackPower = (int)iBaseRangedAttackPower_DIV_14 * 14;

            }
            reader.Close();

            return iBaseRangedAttackPower;
        }



        private void btnGenerateScript_Click(object sender, EventArgs e)
        {
            // connect to the database
            MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "server=127.0.0.1;uid=root;" +
                "pwd=root;database=mangosone;";

            conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString);
            try
            {
                conn.Open();
                lblConnectionState.Text = "Connected";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                lblConnectionState.Text = "Failed to Connect";
            }
            // read in data

            // base health
            int iBaseHealth = 0;

            // base mana
            int iBaseMana = 0;
            int iMinMana = 0;
            int iMaxMana = 0;
            double dManaMultiplier = 0;

            double dBaseDamage = 0;
            int iDamageMultiplier = 0; // DamageMultiplyer in creature_tempplate table
            double dDamageVariance = 0; // DamageVariance in creature_tempplate table

            int iBaseMeleeAttackPower = 0; // used for itself (BaseMeleeAttackPower) and for calculating BaseDamge

            int iBaseRangedAttackPower = 0;
            int iBaseRangedAttackTime = 0; // MeleeBaseAttackTime in creature_tempplate table
            int iMinRangedDamage = 0;
            int iMaxRangedDamage = 0;


            int iBaseArmour = 0; // ?????????????????? no info on how to obtain this!

            // SET THIS TO 0 for all records
                
            // Class: Warrior (1)
            // ------------------
            for (int iCreatureLevel = 1; iCreatureLevel <= 60; iCreatureLevel++)
            {                
                // Generate base health
                iBaseHealth = getBaseHealth(conn, MAGE_CLASS, iCreatureLevel);
        //        txtResults.Text += " BASE DMG: " + iBaseHealth + "\r\n";

                // Generate base mana - will be 0 for all records
                iBaseMana = 0;

                // Generate base damage
                dBaseDamage = getBaseDamage(conn, MAGE_CLASS, iCreatureLevel, iDamageMultiplier, dDamageVariance, iBaseMeleeAttackPower);
                txtResults.Text += " BASE DMG: " + dBaseDamage + " --- ";


                // Generate base ranged attack power
                iBaseRangedAttackPower = getBaseRangedAttackPower(conn, MAGE_CLASS, iCreatureLevel, iDamageMultiplier, dDamageVariance, dBaseDamage);
                txtResults.Text += " BASE RANGED ATTACK POWER: " + iBaseRangedAttackPower + "\r\n";

            }


            /*
            // Class: Warrior (1)
            // ---- BASE MELEE ATTACK POWER ------
            // simples
            for (int i = 1; i <= 60; i++)
            {
                // Generate base damage

                sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = 1 AND MinLevel = " + i.ToString() + " AND MinLevel = MaxLevel AND MinLevelHealth = MaxLevelHealth LIMIT 1 ";

                cmd = new MySqlCommand(sqlScript, conn);

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    txtResults.Text += "" + i + " " + reader.GetString("Entry") + " ";

                    iBaseMeleeAttackPower = reader.GetInt32("MeleeAttackPower");
                    txtResults.Text += " Base Melee AP: " + iBaseMeleeAttackPower;
                }

            }
             * */






                    // Generate base damage

                    // Generate base melee attack power

                    // Generate base ranged attack power

                    // Generate base armour


            // Generate base mana



            // Class: Paladin (2)
            
            // Class: Rogue (4)

            // Class: Mage (8)


            

        }
    }
}
