# import os
# import unittest
# from appium import webdriver
# from altunityrunner import AltrunUnityDriver
# import xmlrunner
# import time
# from pages.base import CZBTests
# import re
# import sys

# from pages.gameplay_page import Gameplay_Page
# from pages.main_menu_page import Main_Menu_Page
# from pages.open_packs_page import Open_Packs_Page
# from pages.match_selection_page import Match_Selection_Page
# from pages.deck_selection_page import Deck_Selection_Page
# from pages.overlord_selection_page import Overlord_Selection_Page
# from pages.overlord_ability_popup_page import Overlord_Ability_Popup_Page
# from pages.horde_editing_page import Horde_Editing_Page
# reload(sys)
# sys.setdefaultencoding('utf8')


# class CZBTutorialTests(CZBTests):

#     def setUp(self):
#         super(CZBTutorialTests, self).setUp()
    
# #     # def test_first_gameplay_tutorial(self):
# #     #     questionPopUp=self.altdriver.wait_for_element('QuestionPopup(Clone)')
# #     #     self.altdriver.find_element(questionPopUp.name+'/Button_Yes').mobile_tap()

# #     #     gameplay_page=Gameplay_Page(self.altdriver,self.driver)


# #     #     self.altdriver.wait_for_current_scene_to_be('GAMEPLAY')
# #     #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
# #     #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
        
# #     #     self.altdriver.wait_for_element('TutorialDescriptionTooltip(Clone)')
# #     #     time.sleep(2)
        
# #     #     board = self.altdriver.wait_for_element('PlayerBoard')
# #     #     board.mobile_tap()

# #     #     self.altdriver.wait_for_element('CreatureCard(Clone)')
# #     #     time.sleep(4)


# #     #     gameplay_page.swipe_card_from_hand_to_board(0)
# #     #     gameplay_page.end_turn_and_wait_for_your_turn()

# #     #     gameplay_page.swipe_board_card_to_opponent_creature(0,0)
       
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)

# #     #     board.mobile_tap()

# #     #     gameplay_page.end_turn_and_wait_for_your_turn()

# #     #     gameplay_page.swipe_board_card_to_opponent_face(1)
# #     #     gameplay_page.swipe_board_card_to_opponent_face(0)
        
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)
        

# #     #     gameplay_page.end_turn_and_wait_for_your_turn()
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)


       
# #     #     time.sleep(1)
# #     #     gameplay_page.opponent_face.mobile_tap()
# #     #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()

    

# #     # def test_second_gameplay_tutorial(self):
# #     #     self.jump_to_tutorial(2)
# #     #     gameplay_page=Gameplay_Page(self.altdriver,self.driver)

# #     #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
# #     #     gameplay_page.end_turn_and_wait_for_your_turn()
# #     #     gameplay_page.player_board.mobile_tap()
# #     #     gameplay_page.player_board.mobile_tap()
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)
# #     #     gameplay_page.end_turn_and_wait_for_your_turn()
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)
# #     #     gameplay_page.player_board.mobile_tap()
# #     #     gameplay_page.player_board.mobile_tap()
# #     #     gameplay_page.swipe_board_card_to_opponent_creature(1,0)
# #     #     gameplay_page.swipe_board_card_to_opponent_face(0)
# #     #     gameplay_page.swipe_primary_spell_to_opponent_face()

# #     #     self.altdriver.wait_for_element('YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue').mobile_tap()



# #     # def test_third_gameplay_tutorial(self):
# #     #     self.jump_to_tutorial(4)
# #     #     gameplay_page=Gameplay_Page(self.altdriver,self.driver)

# #     #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()

# #     #     gameplay_page.end_turn_and_wait_for_your_turn()
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)
# #     #     gameplay_page.end_turn_and_wait_for_your_turn()
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)   
        
# #     #     time.sleep(4)
# #     #     gameplay_page.player_board.mobile_tap()
# #     #     time.sleep(4)
# #     #     gameplay_page.player_board.mobile_tap()
# #     #     time.sleep(1)        

# #     #     gameplay_page.swipe_board_card_to_opponent_creature(0,1)
# #     #     gameplay_page.swipe_board_card_to_opponent_creature(0,0)
# #     #     gameplay_page.end_turn_and_wait_for_your_turn()
# #     #     gameplay_page.swipe_card_from_hand_to_board(0)
# #     #     gameplay_page.swipe_board_card_to_opponent_creature(1,0)
# #     #     gameplay_page.player_board.mobile_tap()
# #     #     gameplay_page.swipe_board_card_to_opponent_face(0)
        

# #     #     self.altdriver.wait_for_element('YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue').mobile_tap()
        


# #     # def test_fourth_gameplay_tutorial(self):
# #     #     self.jump_to_tutorial(6)
# #     #     gameplay_page=Gameplay_Page(self.altdriver,self.driver)

# #     #     self.altdriver.wait_for_element('Description/Button_Ok').mobile_tap()
# #     #     self.altdriver.wait_for_element('TutorialDescriptionTooltip(Clone)')
# #     #     time.sleep(2)
# #     #     gameplay_page.player_board.mobile_tap()
# #     #     gameplay_page.end_turn_and_wait_for_your_turn()

# #     #     gameplay_page.swipe_card_from_hand_to_board(0)

# #     #     gameplay_page.player_board.mobile_tap()
# #     #     gameplay_page.swipe_board_card_to_opponent_creature(0,1)

# #     #     gameplay_page.end_turn_and_wait_for_your_turn()
# #     #     gameplay_page.swipe_card_from_hand_to_board(1)

# #     #     gameplay_page.player_board.mobile_tap()
# #     #     gameplay_page.swipe_card_from_hand_to_board(1)
# #     #     gameplay_page.swipe_board_card_to_opponent_creature(1,2)

# #     #     gameplay_page.swipe_board_card_to_opponent_creature(0,0)
# #     #     gameplay_page.end_turn_and_wait_for_your_turn()

# #     #     gameplay_page.swipe_card_from_hand_to_board(0)
# #     #     gameplay_page.get_opponent_board_creatures()[0].mobile_tap()
# #     #     time.sleep(3)

# #     #     gameplay_page.swipe_board_card_to_opponent_face(0)
# #     #     self.altdriver.wait_for_element('YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue').mobile_tap()

#     def test_deck_construction_tutorial(self):
#         self.jump_to_tutorial(1)
#         Main_Menu_Page(self.altdriver).press_open_packs_button()
#         open_packs_page=Open_Packs_Page(self.altdriver,self.driver)
#         open_packs_page.open_pack()
#         open_packs_page.press_back_button()
#         Main_Menu_Page(self.altdriver).press_play_button()
#         Match_Selection_Page(self.altdriver).press_solo_button()
#         Deck_Selection_Page(self.altdriver).create_new_deck_tutorial()
#         Overlord_Selection_Page(self.altdriver).press_continue()
#         Overlord_Ability_Popup_Page(self.altdriver).press_continue()
#         horde_editing_page=Horde_Editing_Page(self.altdriver,self.driver)
#         horde_editing_page.add_cards_to_horde(1)
#         cards=horde_editing_page.get_cards_shown_in_horde_panel()
#         horde_editing_page.double_tap(cards[0])
#         cards=horde_editing_page.get_cards_shown_in_army_panel()
#         horde_editing_page.double_tap(cards[0])
#         horde_editing_page.add_cards_to_horde(9)
#         horde_editing_page.press_save()
#         Deck_Selection_Page(self.altdriver).start_match()

#         Gameplay_Page(self.altdriver,self.driver)


#         ##Deck tutorial 2
#         self.jump_to_tutorial_from_another_tutorial(3)
#         open_packs_page=Open_Packs_Page(self.altdriver,self.driver)
#         open_packs_page.open_pack()
#         open_packs_page.press_back_button()
#         Main_Menu_Page(self.altdriver).press_play_button()
#         Match_Selection_Page(self.altdriver).press_solo_button()
#         Deck_Selection_Page(self.altdriver).press_edit_button()
#         horde_editing_page=Horde_Editing_Page(self.altdriver,self.driver)
#         horde_editing_page.add_cards_to_horde(5)
#         horde_editing_page.press_save()
#         Deck_Selection_Page(self.altdriver).start_match()

#         Gameplay_Page(self.altdriver,self.driver)

#         ##Deck tutorial 3
#         self.jump_to_tutorial_from_another_tutorial(5)
#         open_packs_page=Open_Packs_Page(self.altdriver,self.driver)
#         open_packs_page.open_pack()
#         open_packs_page.press_back_button()
#         Main_Menu_Page(self.altdriver).press_play_button()
#         Match_Selection_Page(self.altdriver).press_solo_button()
#         Deck_Selection_Page(self.altdriver).press_edit_button()
#         horde_editing_page=Horde_Editing_Page(self.altdriver,self.driver)
#         horde_editing_page.add_cards_to_horde(5)
#         horde_editing_page.press_save()
#         Deck_Selection_Page(self.altdriver).start_match()

#         Gameplay_Page(self.altdriver,self.driver)

#         ##Deck tutorial 4
#         self.jump_to_tutorial_from_another_tutorial(7)
#         open_packs_page=Open_Packs_Page(self.altdriver,self.driver)
#         open_packs_page.open_pack()
#         open_packs_page.open_pack()
#         open_packs_page.press_back_button()
#         Main_Menu_Page(self.altdriver).press_play_button()
#         Match_Selection_Page(self.altdriver).press_solo_button()
#         Deck_Selection_Page(self.altdriver).press_edit_button()
#         horde_editing_page=Horde_Editing_Page(self.altdriver,self.driver)
#         horde_editing_page.add_cards_to_horde(10)
#         horde_editing_page.press_save()
#         Deck_Selection_Page(self.altdriver).start_match()

#         Gameplay_Page(self.altdriver,self.driver)
        







# if __name__ == '__main__':
#     unittest.main(testRunner=xmlrunner.XMLTestRunner(output='test-reports'))


            
