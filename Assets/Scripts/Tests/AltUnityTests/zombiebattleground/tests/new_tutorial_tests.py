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
    
    def test_first_gameplay_tutorial(self):
        questionPopUp=self.altdriver.wait_for_element('QuestionPopup(Clone)')
        self.altdriver.find_element(questionPopUp.name+'/Button_Yes').mobile_tap()

        gameplay_page=Gameplay_Page(self.altdriver,self.driver)


        self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
        
        self.altdriver.wait_for_element('TutorialDescriptionTooltip(Clone)')
        time.sleep(2)
        
        board = self.altdriver.wait_for_element('PlayerBoard')
        board.mobile_tap()

        self.altdriver.wait_for_element('CreatureCard(Clone)')
        time.sleep(4)


        gameplay_page.swipe_card_from_hand_to_board(0)
        gameplay_page.end_turn_and_wait_for_your_turn()

        gameplay_page.swipe_board_card_to_opponent_creature(0,0)
       
        gameplay_page.swipe_card_from_hand_to_board(0)

        board.mobile_tap()

        gameplay_page.end_turn_and_wait_for_your_turn()

        gameplay_page.swipe_board_card_to_opponent_face(1)
        gameplay_page.swipe_board_card_to_opponent_face(0)
        
        gameplay_page.swipe_card_from_hand_to_board(0)
        

        gameplay_page.end_turn_and_wait_for_your_turn()
        gameplay_page.swipe_card_from_hand_to_board(0)


       
        time.sleep(1)
        gameplay_page.opponent_face.mobile_tap()
        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()

    

    def test_second_gameplay_tutorial(self):
        self.jump_to_tutorial(2)
        gameplay_page=Gameplay_Page(self.altdriver,self.driver)

        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
        gameplay_page.end_turn_and_wait_for_your_turn()
        gameplay_page.player_board.mobile_tap()
        gameplay_page.player_board.mobile_tap()
        gameplay_page.swipe_card_from_hand_to_board(0)
        gameplay_page.end_turn_and_wait_for_your_turn()
        gameplay_page.swipe_card_from_hand_to_board(0)
        gameplay_page.player_board.mobile_tap()
        gameplay_page.player_board.mobile_tap()
        gameplay_page.swipe_board_card_to_opponent_creature(1,0)
        gameplay_page.swipe_board_card_to_opponent_face(0)
        gameplay_page.swipe_primary_spell_to_opponent_face()

        self.altdriver.wait_for_element('YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue').mobile_tap()



    def test_third_gameplay_tutorial(self):
        self.jump_to_tutorial(4)
        gameplay_page=Gameplay_Page(self.altdriver,self.driver)

        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()

        gameplay_page.end_turn_and_wait_for_your_turn()
        gameplay_page.swipe_card_from_hand_to_board(0)
        gameplay_page.swipe_card_from_hand_to_board(0)
        gameplay_page.end_turn_and_wait_for_your_turn()
        gameplay_page.swipe_card_from_hand_to_board(0)   
        
        time.sleep(4)
        gameplay_page.player_board.mobile_tap()
        time.sleep(4)
        gameplay_page.player_board.mobile_tap()
        time.sleep(1)        

        gameplay_page.swipe_board_card_to_opponent_creature(0,1)
        gameplay_page.swipe_board_card_to_opponent_creature(0,0)
        gameplay_page.end_turn_and_wait_for_your_turn()
        gameplay_page.swipe_card_from_hand_to_board(0)
        gameplay_page.swipe_board_card_to_opponent_creature(1,0)
        gameplay_page.player_board.mobile_tap()
        gameplay_page.swipe_board_card_to_opponent_face(0)
        

        self.altdriver.wait_for_element('YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue').mobile_tap()
        


    def test_fourth_gameplay_tutorial(self):
        self.jump_to_tutorial(6)
        gameplay_page=Gameplay_Page(self.altdriver,self.driver)

        self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
        self.altdriver.wait_for_element('TutorialDescriptionTooltip(Clone)')
        time.sleep(2)
        gameplay_page.player_board.mobile_tap()
        gameplay_page.end_turn_and_wait_for_your_turn()

        gameplay_page.swipe_card_from_hand_to_board(0)

        gameplay_page.player_board.mobile_tap()
        gameplay_page.swipe_board_card_to_opponent_creature(0,1)

        gameplay_page.end_turn_and_wait_for_your_turn()
        gameplay_page.swipe_card_from_hand_to_board(1)

        gameplay_page.player_board.mobile_tap()
        gameplay_page.swipe_card_from_hand_to_board(1)
        gameplay_page.swipe_board_card_to_opponent_creature(1,2)

        gameplay_page.swipe_board_card_to_opponent_creature(0,0)
        gameplay_page.end_turn_and_wait_for_your_turn()

        gameplay_page.swipe_card_from_hand_to_board(0)
        gameplay_page.get_opponent_board_creatures()[0].mobile_tap()
        time.sleep(3)

        gameplay_page.swipe_board_card_to_opponent_face(0)
        self.altdriver.wait_for_element('YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue').mobile_tap()






if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))


             







        
         


   

if __name__ == '__main__':
    unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))
