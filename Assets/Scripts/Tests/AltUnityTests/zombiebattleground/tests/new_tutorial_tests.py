import os
import unittest
from appium import webdriver
from altunityrunner import AltrunUnityDriver
import xmlrunner
import time
from pages.base import CZBTests
import re
import sys

from pages.gameplay_page import Gameplay_Page
reload(sys)
sys.setdefaultencoding('utf8')


class CZBTutorialTests(CZBTests):

    def setUp(self):
        super(CZBTutorialTests, self).setUp()
    
    # def test_first_tutorial(self):
    #     questionPopUp=self.altdriver.wait_for_element('QuestionPopup(Clone)')
    #     self.altdriver.find_element(questionPopUp.name+'/Button_Yes').mobile_tap()


    #     self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
    #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
    #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
        
    #     self.altdriver.wait_for_element('TutorialDescriptionTooltip(Clone)')
    #     time.sleep(2)
        
    #     board = self.altdriver.wait_for_element('PlayerBoard')
    #     board.mobile_tap()

    #     self.altdriver.wait_for_element('CreatureCard(Clone)')
    #     time.sleep(4)


    #     card=self.altdriver.wait_for_element('CreatureCard(Clone)')
    #     cardGoo = self.altdriver.wait_for_element(
    #         'id('+str(card.id)+')/GooText')
    #     self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
    #                       int(board.x), int(board.mobileY), 2000)

        
    #     self.end_turn_and_wait_for_your_turn()

    #     playerCard=self.altdriver.wait_for_element('PlayerBoard/BoardCreature(Clone)')
    #     enemyCard=self.altdriver.wait_for_element('OpponentBoard/BoardCreature(Clone)')

    #     self.driver.swipe(int(playerCard.x), int(playerCard.mobileY),
    #                       int(enemyCard.x), int(enemyCard.mobileY), 2000)
       
    #     self.altdriver.wait_for_element('CreatureCard(Clone)')
    #     time.sleep(3)
    #     card=self.altdriver.wait_for_element('CreatureCard(Clone)')
    #     cardGoo = self.altdriver.wait_for_element(
    #         'id('+str(card.id)+')/GooText')
    #     board = self.altdriver.wait_for_element('PlayerBoard')
    #     self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
    #                       int(board.x), int(board.mobileY), 2000)

    #     board.mobile_tap()

    #     self.end_turn_and_wait_for_your_turn()


    #     playerCards=self.altdriver.find_elements('PlayerBoard/BoardCreature(Clone)')
    #     oponentFace=self.altdriver.wait_for_element('Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/OverlordImage')
    
    #     self.driver.swipe(int(playerCards[1].x), int(playerCards[1].mobileY),
    #                       int(oponentFace.x), int(oponentFace.mobileY), 2000)
    #     time.sleep(1)
    #     self.driver.swipe(int(playerCards[0].x), int(playerCards[0].mobileY),
    #                       int(oponentFace.x), int(oponentFace.mobileY), 2000)
    #     time.sleep(2)
    #     card=self.altdriver.wait_for_element('CreatureCard(Clone)')
    #     cardGoo = self.altdriver.wait_for_element(
    #         'id('+str(card.id)+')/GooText')
    #     self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
    #                       int(board.x), int(board.mobileY), 2000)

    #     self.end_turn_and_wait_for_your_turn()


    #     card=self.altdriver.wait_for_element('ItemCard(Clone)')
    #     cardGoo = self.altdriver.wait_for_element(
    #         'id('+str(card.id)+')/GooText')
    #     board = self.altdriver.wait_for_element('PlayerBoard')
    #     self.driver.swipe(int(cardGoo.x), int(cardGoo.mobileY),
    #                       int(board.x), int(board.mobileY), 2000)
    #     time.sleep(1)
    #     oponentFace.mobile_tap()
    #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()

    # def end_turn_and_wait_for_your_turn(self):
    #     self.altdriver.wait_for_element('EndTurnButton/_1_btn_endturn').mobile_tap()
    #     self.altdriver.wait_for_element(
    #         'EndTurnButton/_1_btn_endturn/EndTurnGlowEffect', timeout=60)
    #     time.sleep(4)
    def test_third_tutorial(self):
        self.skip_tutorials()
        self.altdriver.wait_for_element('HiddenUI')
        self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','true','UnityEngine.CoreModule')
        self.altdriver.find_element('InputField').set_component_property('UnityEngine.UI.InputField','text',4,'UnityEngine.UI')
        self.altdriver.find_element('JumpToTutorial').mobile_tap()
        self.altdriver.find_element('Root',enabled=False).call_component_method('UnityEngine.GameObject','SetActive','false','UnityEngine.CoreModule')

        gameplay_page=Gameplay_Page(self.altdriver,self.driver)

        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()

        gameplay_page.end_turn_and_wait_for_your_turn()

        hand_cards=gameplay_page.get_cards_that_are_in_hand()
        gameplay_page.swipe_card_from_hand_to_board(hand_cards[0])
        time.sleep(2)
        gameplay_page.swipe_card_from_hand_to_board(hand_cards[1])

        gameplay_page.end_turn_and_wait_for_your_turn()
        time.sleep(2)

        hand_cards=gameplay_page.get_cards_that_are_in_hand()
        gameplay_page.swipe_card_from_hand_to_board(hand_cards[0])
        
        time.sleep(4)
        gameplay_page.player_board.mobile_tap()
        time.sleep(4)
        gameplay_page.player_board.mobile_tap()
        time.sleep(1)

        player_board_creature=gameplay_page.get_player_board_creatures()
        enemy_board_creature=gameplay_page.get_opponent_board_creatures()
        player_board_creature[0].mobile_dragToElement(enemy_board_creature[1],2000)
        time.sleep(5)
        
        player_board_creature=gameplay_page.get_player_board_creatures()
        enemy_board_creature=gameplay_page.get_opponent_board_creatures()
        player_board_creature[0].mobile_dragToElement(enemy_board_creature[0],2000)
        time.sleep(5)

        gameplay_page.end_turn_and_wait_for_your_turn()

        hand_cards=gameplay_page.get_cards_that_are_in_hand()
        gameplay_page.swipe_card_from_hand_to_board(hand_cards[0])
        time.sleep(3)

        player_board_creature=gameplay_page.get_player_board_creatures()
        enemy_board_creature=gameplay_page.get_opponent_board_creatures()
        player_board_creature[1].mobile_dragToElement(enemy_board_creature[0],2000)
        time.sleep(4)

        gameplay_page.player_board.mobile_tap()
        player_board_creature=gameplay_page.get_player_board_creatures()
        gameplay_page.swipe_card_to_opponent_face(player_board_creature[0])
        

        self.altdriver.wait_for_element('YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue').mobile_tap()
        




if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))


             







        
         


   

if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
