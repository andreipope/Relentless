import sys
import unittest

from appium import webdriver
from altunityrunner import AltElement, AltrunUnityDriver, AltUnityException
from base import CZBTests
import time
import xmlrunner


reload(sys)
sys.setdefaultencoding('utf8')
cards=[]
class CZBPlayTests(CZBTests):

    def setUp(self):
        super(CZBPlayTests, self).setUp()
        self.altdriver.wait_for_element('Button_Skip').mobile_tap()
        time.sleep(1)
        self.altdriver.wait_for_element("Button_Yes").mobile_tap()
        self.altdriver.wait_for_element_to_not_be_present("LoadingGameplayPopup(Clone)")
        self.altdriver.wait_for_element('Button_Back').mobile_tap()


    def test_start_match_versus_ai(self):
        #enter Deck Selection

        self.altdriver.wait_for_element('Button_Play').mobile_tap()
        self.altdriver.wait_for_element('Button_SoloMode').mobile_tap()


        cards=self.get_list_of_card_in_deck()
       
        # self.altdriver.wait_for_element('Button_SoloMode').mobile_tap()
        
        while cards.count!=0:
            # Play match until you played all the card in the deck once
            self.altdriver.wait_for_element('Button_Battle').mobile_tap()
            self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')

            self.altdriver.wait_for_element('Button_Keep').mobile_tap()
            
            while self.game_not_ended():
                self.your_turn()


    def your_turn(self):
        self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn/EndTurnGlowEffect',timeout=60)
        self.check_player_goo_bottle_to_be_refilled()
        time.sleep(3)
        self.play_a_card()
        self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn').mobile_tap()
        time.sleep(1)
        self.check_opponent_goo_bottle_to_be_refilled()
        
        
    def get_player_defence(self):
        return int(self.altdriver.wait_for_element('Player/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText').get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro'))
    
    def get_opponent_defence(self):
        return int(self.altdriver.wait_for_element('Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText').get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro'))
    def get_player_goo(self):
        return int(self.altdriver.wait_for_element('Player/OverlordArea/RegularModel/RegularPosition/Gauge/CZB_3D_Overlord_gauge_LOD0/Text').get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro').split('/')[0])

    def check_player_goo_bottle_to_be_refilled(self):
        number_of_bottles=len(self.altdriver.find_elements('PlayerManaBar/BottleGoo'))
        number_of_goo=len(self.altdriver.find_elements('PlayerManaBar/BottleGoo/New Sprite Mask/goo'))
        self.assertEqual(number_of_bottles,number_of_goo)
    
    def check_opponent_goo_bottle_to_be_refilled(self):
        number_of_bottles=len(self.altdriver.find_elements('OpponentManaBar/BottleGoo'))
        number_of_goo=len(self.altdriver.find_elements('OpponentManaBar/BottleGoo/New Sprite Mask/goo'))
        self.assertEqual(number_of_bottles,number_of_goo)
        
    def game_not_ended(self):
        timeout=40
        while True:
            try:
                self.altdriver.find_element('YouLosePopup(Clone)')
                return False
            except Exception:
                print('YouLosePopup not found')
            try:
                self.altdriver.find_element('YouWinPopup(Clone)')
                return False
            except Exception:
                print('YouWinPopup not found')
            try:
                self.altdriver.find_element('EndTurnButton/_1_btn_endturn/EndTurnGlowEffect')
                return True
            except Exception:
                print('EndTurnButton not found')
            time.sleep(3)
            timeout=timeout-3
            if(timeout<=0):
                return False

    def play_a_card(self):
        global cards
        handCards=[]
        handCards=self.altdriver.find_elements("CreatureCard(Clone)",'MainCamera')
        itemCards=self.altdriver.find_elements("ItemCard(Clone)","MainCamera")
        handCards=handCards+itemCards
        totalGoo=self.get_player_goo()
        
        for card in handCards:
            if(self.check_card_is_playable(card,totalGoo)):
                print('play a card')
                print(cards.count)
                print('remove a card from the list')
                print(cards)
                cardName=self.altdriver.wait_for_element('id('+str(card.id)+')/TitleText').get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro')
                cards.remove(cardName)
                print(cards.count)
                self.move_card_from_hand_to_battlefield(card)
                break
            
    def check_card_is_playable(self,card,totalGoo):
        cardGooValue=self.altdriver.wait_for_element('id('+str(card.id)+')/GooText').get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro')
        if(int(cardGooValue)<=totalGoo):#TODO Check if is there space on the board 
            # self.assertIsNotNone(self.altdriver.find_element('id('+str(card.id)+')/GlowContainer/Glow'))
            print('true')
            return True
        return False
    
    def move_card_from_hand_to_battlefield(self,card):
        playerBoard=self.altdriver.find_element('PlayerBoard')
        gooText=self.altdriver.find_element('id('+str(card.id)+')/GooText','MainCamera')
        print(int(gooText.x))
        print(int(gooText.mobileY))
        self.driver.swipe(int(gooText.x),int(gooText.mobileY),int(playerBoard.x),int(playerBoard.mobileY),2000)
        localtime = time.localtime(time.time())
        print "Local current time :", localtime
        time.sleep(3)
        localtime = time.localtime(time.time())
        print "Local current time :", localtime

    def get_list_of_card_in_deck(self):
        global cards
        #Deck selection page
        self.altdriver.wait_for_element('Button_Edit').mobile_tap()

        #Horde Edititing Page
        self.altdriver.wait_for_element('HordeEditingPage(Clone)')

        creatureCards=self.altdriver.find_elements('Horde/Cards/CreatureCard(Clone)/TitleText',enabled=False)
        for cardName in creatureCards:
            cards.append(cardName.get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro'))
        itemCards=self.altdriver.find_elements('Horde/Cards/ItemCard(Clone)/TitleText',enabled=False)
        for cardName in itemCards:
            cards.append(cardName.get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro'))

        self.altdriver.wait_for_element('Button_Save').mobile_tap()
        return cards
       
        
       






        # All the cards in the horde are instatiated but are disable so don't need to go through every page
        # while True:
        #     card=self.altdriver.find_element('Horde/Cards/CreatureCard(Clone)/TitleText')
        #     if card==None:
        #         card=self.altdriver.find_element('Horde/Cards/ItemCard(Clone)/TitleText')
            
        #     if self.is_card_in_the_list(cards,card):
        #         break

        #     rowCards=self.altdriver.find_elements('Horde/Cards/CreatureCard(Clone)/TitleText')
        
        #     for cardName in rowCards:
        #         cards.append(cardName.get_component_property('TMPro.TextMeshPro','text','Unity.TextMeshPro'))
            
        #     self.altdriver.wait_for_element('Horde/ArrowRightButton').mobile_tap()
        # for cardName in cards:
        #     print(cardName)

    # def is_card_in_the_list(self,cards,card):
    #     for cardName in cards :
    #         if cardName==card.name:
    #             return True
    #     return False


        





        

    

if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))